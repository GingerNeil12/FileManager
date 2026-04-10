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
public class ApplicationUserRepositoryTests
{
    private static MsSqlContainer _msSqlContainer;
    private static ApplicationDbContext _sharedDbContext;

    private IDbContextTransaction _transaction;
    private ILogger<ApplicationUserRepository> _mockLogger;
    private ApplicationUserRepository _sut;

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
        _mockLogger = Substitute.For<ILogger<ApplicationUserRepository>>();
        _sut = new ApplicationUserRepository(_sharedDbContext, _mockLogger);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _sharedDbContext.ChangeTracker.Clear();
    }

    [Test]
    public async Task GetByAsync_WhenUserExists_ReturnsSuccess()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("provider-id", "email", "given_name", "family_name", null);
        await SeedAsync(user);

        // Act
        Result<ApplicationUser, Error> result = await _sut.GetByAsync(user.ExternalProviderId, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task GetByAsync_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const string nonExistentProviderId = "non-existent-provider-id";

        // Act
        Result<ApplicationUser, Error> result = await _sut.GetByAsync(nonExistentProviderId, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<NotFoundError>());
    }

    [Test]
    public void GetByAsync_WhenExceptionOccurs_Rethrows()
    {
        // Arrange
        ApplicationUserRepository sut = CreateSutWithDisposedContext();

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.GetByAsync("provider-id", CancellationToken.None));
    }

    [Test]
    public void GetByAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        ILogger<ApplicationUserRepository> mockLogger = Substitute.For<ILogger<ApplicationUserRepository>>();
        ApplicationUserRepository sut = CreateSutWithDisposedContext(mockLogger);

        // Act
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.GetByAsync("provider-id", CancellationToken.None));

        // Assert
        AssertLoggedError(mockLogger);
    }

    [Test]
    public async Task SaveAsync_WhenCalled_PersistsUser()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("provider-id", "email", "given_name", "family_name", null);

        // Act
        await _sut.SaveAsync(user, CancellationToken.None);

        // Assert
        ApplicationUser saved = await _sharedDbContext.Users.FindAsync(user.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public void SaveAsync_WhenExceptionOccurs_Rethrows()
    {
        // Arrange
        ApplicationUserRepository sut = CreateSutWithDisposedContext();
        ApplicationUser user = ApplicationUser.Create("provider-id", "email", "given_name", "family_name", null);

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.SaveAsync(user, CancellationToken.None));
    }

    [Test]
    public void SaveAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        ILogger<ApplicationUserRepository> mockLogger = Substitute.For<ILogger<ApplicationUserRepository>>();
        ApplicationUserRepository sut = CreateSutWithDisposedContext(mockLogger);
        ApplicationUser user = ApplicationUser.Create("provider-id", "email", "given_name", "family_name", null);

        // Act
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.SaveAsync(user, CancellationToken.None));

        // Assert
        AssertLoggedError(mockLogger);
    }

    private async Task SeedAsync(ApplicationUser user)
    {
        _sharedDbContext.Users.Add(user);
        await _sharedDbContext.SaveChangesAsync();
        _sharedDbContext.ChangeTracker.Clear();
    }

    private ApplicationUserRepository CreateSutWithDisposedContext(ILogger<ApplicationUserRepository> logger = null)
    {
        DbContextOptions options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .Options;
        ApplicationDbContext disposedContext = new(options);
        disposedContext.Dispose();
        return new ApplicationUserRepository(disposedContext, logger ?? _mockLogger);
    }

    private static void AssertLoggedError(ILogger<ApplicationUserRepository> logger)
    {
        bool wasLogged = logger.ReceivedCalls()
            .Any(c => c.GetMethodInfo().Name == "Log"
                   && c.GetArguments()[0] is LogLevel level
                   && level == LogLevel.Error
                   && c.GetArguments()[3] is Exception);
        Assert.That(wasLogged, Is.True);
    }
}
