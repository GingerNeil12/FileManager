#nullable disable

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Persistence.Repositories;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Testcontainers.Azurite;

namespace FileManager.Persistence.Tests.Repositories;

[TestFixture]
public class BlobRepositoryTests
{
    private static AzuriteContainer _azuriteContainer;

    private BlobContainerClient _containerClient;
    private ILogger<BlobRepository> _mockLogger;
    private BlobRepository _sut;

    private const string TEST_CONTAINER_NAME = "test-container";
    private const string TEST_BLOB_LOCATION = "test-file.txt";
    private const string NON_EXISTENT_BLOB_LOCATION = "non-existent.txt";

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:latest")
            .WithCommand("--skipApiVersionCheck")
            .Build();
        await _azuriteContainer.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _azuriteContainer.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        _mockLogger = Substitute.For<ILogger<BlobRepository>>();
        BlobServiceClient serviceClient = new(_azuriteContainer.GetConnectionString());
        _containerClient = serviceClient.GetBlobContainerClient(TEST_CONTAINER_NAME);
        await _containerClient.CreateIfNotExistsAsync();
        _sut = new BlobRepository(_containerClient, _mockLogger);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _containerClient.DeleteIfExistsAsync();
    }

    [Test]
    public async Task UploadContentAsync_WhenCalled_UploadsBlob()
    {
        // Arrange
        using MemoryStream stream = new("hello world"u8.ToArray());

        // Act
        await _sut.UploadContentAsync(TEST_BLOB_LOCATION, stream, CancellationToken.None);

        // Assert
        BlobClient blobClient = _containerClient.GetBlobClient(TEST_BLOB_LOCATION);
        Response<bool> exists = await blobClient.ExistsAsync();
        Assert.That(exists.Value, Is.True);
    }

    [Test]
    public void UploadContentAsync_WhenExceptionOccurs_Rethrows()
    {
        // Arrange
        using MemoryStream stream = new();
        BlobRepository sut = CreateSutWithFailingUpload();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.UploadContentAsync(TEST_BLOB_LOCATION, stream, CancellationToken.None));
    }

    [Test]
    public void UploadContentAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        using MemoryStream stream = new();
        BlobRepository sut = CreateSutWithFailingUpload();

        // Act
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.UploadContentAsync(TEST_BLOB_LOCATION, stream, CancellationToken.None));

        // Assert
        AssertLoggedError();
    }

    [Test]
    public async Task DownloadContentAsync_WhenBlobExists_ReturnsStream()
    {
        // Arrange
        byte[] content = "hello world"u8.ToArray();
        await UploadBlobAsync(TEST_BLOB_LOCATION, content);

        // Act
        Result<Stream, Error> result = await _sut.DownloadContentAsync(TEST_BLOB_LOCATION, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        using MemoryStream buffer = new();
        await result.Value.CopyToAsync(buffer);
        Assert.That(buffer.ToArray(), Is.EqualTo(content));
    }

    [Test]
    public async Task DownloadContentAsync_WhenBlobDoesNotExist_ReturnsNotFoundError()
    {
        // Act
        Result<Stream, Error> result = await _sut.DownloadContentAsync(NON_EXISTENT_BLOB_LOCATION, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<NotFoundError>());
    }

    [Test]
    public void DownloadContentAsync_WhenExceptionOccurs_Rethrows()
    {
        // Arrange
        BlobRepository sut = CreateSutWithFailingDownload();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.DownloadContentAsync(TEST_BLOB_LOCATION, CancellationToken.None));
    }

    [Test]
    public void DownloadContentAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        BlobRepository sut = CreateSutWithFailingDownload();

        // Act
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.DownloadContentAsync(TEST_BLOB_LOCATION, CancellationToken.None));

        // Assert
        AssertLoggedError();
    }

    [Test]
    public async Task DeleteContentAsync_WhenBlobExists_DeletesBlob()
    {
        // Arrange
        await UploadBlobAsync(TEST_BLOB_LOCATION, "hello"u8.ToArray());

        // Act
        await _sut.DeleteContentAsync(TEST_BLOB_LOCATION, CancellationToken.None);

        // Assert
        BlobClient blobClient = _containerClient.GetBlobClient(TEST_BLOB_LOCATION);
        Response<bool> exists = await blobClient.ExistsAsync();
        Assert.That(exists.Value, Is.False);
    }

    [Test]
    public void DeleteContentAsync_WhenBlobDoesNotExist_Succeeds()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(() => _sut.DeleteContentAsync(NON_EXISTENT_BLOB_LOCATION, CancellationToken.None));
    }

    [Test]
    public void DeleteContentAsync_WhenExceptionOccurs_Rethrows()
    {
        // Arrange
        BlobRepository sut = CreateSutWithFailingDelete();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.DeleteContentAsync(TEST_BLOB_LOCATION, CancellationToken.None));
    }

    [Test]
    public void DeleteContentAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        BlobRepository sut = CreateSutWithFailingDelete();

        // Act
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.DeleteContentAsync(TEST_BLOB_LOCATION, CancellationToken.None));

        // Assert
        AssertLoggedError();
    }

    private BlobRepository CreateSutWithFailingUpload()
    {
        BlobContainerClient mockContainerClient = Substitute.For<BlobContainerClient>();
        BlobClient mockBlobClient = Substitute.For<BlobClient>();
        mockContainerClient.GetBlobClient(Arg.Any<string>()).Returns(mockBlobClient);
        mockBlobClient
            .UploadAsync(Arg.Any<Stream>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Response<BlobContentInfo>>(new InvalidOperationException("Storage error")));
        return new BlobRepository(mockContainerClient, _mockLogger);
    }

    private BlobRepository CreateSutWithFailingDownload()
    {
        BlobContainerClient mockContainerClient = Substitute.For<BlobContainerClient>();
        BlobClient mockBlobClient = Substitute.For<BlobClient>();
        mockContainerClient.GetBlobClient(Arg.Any<string>()).Returns(mockBlobClient);
        mockBlobClient
            .DownloadStreamingAsync(Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Response<BlobDownloadStreamingResult>>(new InvalidOperationException("Storage error")));
        return new BlobRepository(mockContainerClient, _mockLogger);
    }

    private BlobRepository CreateSutWithFailingDelete()
    {
        BlobContainerClient mockContainerClient = Substitute.For<BlobContainerClient>();
        BlobClient mockBlobClient = Substitute.For<BlobClient>();
        mockContainerClient.GetBlobClient(Arg.Any<string>()).Returns(mockBlobClient);
        mockBlobClient
            .DeleteIfExistsAsync(Arg.Any<DeleteSnapshotsOption>(), Arg.Any<BlobRequestConditions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Response<bool>>(new InvalidOperationException("Storage error")));
        return new BlobRepository(mockContainerClient, _mockLogger);
    }

    private async Task UploadBlobAsync(string location, byte[] content)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(location);
        using MemoryStream stream = new(content);
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    private void AssertLoggedError()
    {
        bool wasLogged = _mockLogger.ReceivedCalls()
            .Any(c => c.GetMethodInfo().Name == "Log"
                   && c.GetArguments()[0] is LogLevel level
                   && level == LogLevel.Error
                   && c.GetArguments()[3] is Exception);
        Assert.That(wasLogged, Is.True);
    }
}
