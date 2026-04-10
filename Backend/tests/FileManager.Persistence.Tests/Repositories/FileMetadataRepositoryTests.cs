#nullable disable

using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;
using FileManager.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

using NSubstitute;

using Testcontainers.MsSql;

namespace FileManager.Persistence.Tests.Repositories;

[TestFixture]
public class FileMetadataRepositoryTests
{
    private static MsSqlContainer _msSqlContainer;
    private static ApplicationDbContext _sharedDbContext;

    private IDbContextTransaction _transaction;
    private ILogger<FileMetadataRepository> _mockLogger;
    private FileMetadataRepository _sut;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await _msSqlContainer.StartAsync();

        DbContextOptions options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .Options;
        _sharedDbContext = new ApplicationDbContext(options);
        await _sharedDbContext.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _sharedDbContext.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        _transaction = await _sharedDbContext.Database.BeginTransactionAsync();
        _mockLogger = Substitute.For<ILogger<FileMetadataRepository>>();
        _sut = new FileMetadataRepository(_sharedDbContext, _mockLogger);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _sharedDbContext.ChangeTracker.Clear();
    }

    [Test]
    public async Task GetByAsync_WhenFileExists_ReturnsSuccess()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("provider-id");
        FileMetadata file = FileMetadata.Create("location/file.txt", "file.txt", 1024, false, user.Id);
        await SeedAsync(user, file);

        // Act
        Result<FileMetadata, Error> result = await _sut.GetByAsync(file.Id, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(file.Id));
    }

    [Test]
    public async Task GetByAsync_WhenFileDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        Result<FileMetadata, Error> result = await _sut.GetByAsync(nonExistentId, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<NotFoundError>());
    }

    [Test]
    public async Task GetByAsync_WhenFileExists_IncludesUploadedBy()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("provider-id");
        FileMetadata file = FileMetadata.Create("location/file.txt", "file.txt", 1024, false, user.Id);
        await SeedAsync(user, file);

        // Act
        Result<FileMetadata, Error> result = await _sut.GetByAsync(file.Id, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UploadedBy.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task GetByAsync_WhenFileHasAssignees_IncludesAssignees()
    {
        // Arrange
        ApplicationUser uploader = ApplicationUser.Create("uploader-id");
        ApplicationUser assignee = ApplicationUser.Create("assignee-id");
        FileMetadata file = FileMetadata.Create("location/file.txt", "file.txt", 1024, false, uploader.Id);
        FileMember member = FileMember.Create(file.Id, assignee.Id);
        _sharedDbContext.Users.Add(uploader);
        _sharedDbContext.Users.Add(assignee);
        _sharedDbContext.FileMetadata.Add(file);
        _sharedDbContext.FileMembers.Add(member);
        await _sharedDbContext.SaveChangesAsync();
        _sharedDbContext.ChangeTracker.Clear();

        // Act
        Result<FileMetadata, Error> result = await _sut.GetByAsync(file.Id, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Assignees, Has.Count.EqualTo(1));
        Assert.That(result.Value.Assignees[0].AssignedToId, Is.EqualTo(assignee.Id));
    }

    [Test]
    public async Task GetByAsync_WhenFileHasNoAssignees_ReturnsEmptyAssigneesList()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("provider-id");
        FileMetadata file = FileMetadata.Create("location/file.txt", "file.txt", 1024, false, user.Id);
        await SeedAsync(user, file);

        // Act
        Result<FileMetadata, Error> result = await _sut.GetByAsync(file.Id, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Assignees, Is.Empty);
    }

    [Test]
    public void GetByAsync_WhenExceptionOccurs_Rethrows()
    {
        // Arrange
        FileMetadataRepository sut = CreateSutWithDisposedContext();

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.GetByAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void GetByAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        ILogger<FileMetadataRepository> mockLogger = Substitute.For<ILogger<FileMetadataRepository>>();
        FileMetadataRepository sut = CreateSutWithDisposedContext(mockLogger);

        // Act
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.GetByAsync(Guid.NewGuid(), CancellationToken.None));

        // Assert
        AssertLoggedError(mockLogger);
    }

    [Test]
    public async Task SaveAsync_WhenCalled_PersistsFileMetadata()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("provider-id");
        _sharedDbContext.Users.Add(user);
        await _sharedDbContext.SaveChangesAsync();
        _sharedDbContext.ChangeTracker.Clear();
        FileMetadata file = FileMetadata.Create("location/file.txt", "file.txt", 1024, false, user.Id);

        // Act
        await _sut.SaveAsync(file, CancellationToken.None);

        // Assert
        FileMetadata saved = await _sharedDbContext.FileMetadata.FindAsync(file.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved.Id, Is.EqualTo(file.Id));
    }

    [Test]
    public void SaveAsync_WhenExceptionOccurs_Rethrows()
    {
        // Arrange
        FileMetadataRepository sut = CreateSutWithDisposedContext();
        FileMetadata file = FileMetadata.Create("location/file.txt", "file.txt", 1024, false, Guid.NewGuid());

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.SaveAsync(file, CancellationToken.None));
    }

    [Test]
    public void SaveAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        ILogger<FileMetadataRepository> mockLogger = Substitute.For<ILogger<FileMetadataRepository>>();
        FileMetadataRepository sut = CreateSutWithDisposedContext(mockLogger);
        FileMetadata file = FileMetadata.Create("location/file.txt", "file.txt", 1024, false, Guid.NewGuid());

        // Act
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.SaveAsync(file, CancellationToken.None));

        // Assert
        AssertLoggedError(mockLogger);
    }

    private async Task SeedAsync(ApplicationUser user, FileMetadata file)
    {
        _sharedDbContext.Users.Add(user);
        _sharedDbContext.FileMetadata.Add(file);
        await _sharedDbContext.SaveChangesAsync();
        _sharedDbContext.ChangeTracker.Clear();
    }

    private FileMetadataRepository CreateSutWithDisposedContext(ILogger<FileMetadataRepository> logger = null)
    {
        DbContextOptions options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .Options;
        ApplicationDbContext disposedContext = new(options);
        disposedContext.Dispose();
        return new FileMetadataRepository(disposedContext, logger ?? _mockLogger);
    }

    private static void AssertLoggedError(ILogger<FileMetadataRepository> logger)
    {
        bool wasLogged = logger.ReceivedCalls()
            .Any(c => c.GetMethodInfo().Name == "Log"
                   && c.GetArguments()[0] is LogLevel level
                   && level == LogLevel.Error
                   && c.GetArguments()[3] is Exception);
        Assert.That(wasLogged, Is.True);
    }
}
