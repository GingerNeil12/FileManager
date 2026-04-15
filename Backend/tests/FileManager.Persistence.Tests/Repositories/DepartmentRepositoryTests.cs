using FileManager.Application.Common.Models;
using FileManager.Application.Common.Models.Specifications;
using FileManager.Domain.Models;
using FileManager.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

using NSubstitute;

using Testcontainers.MsSql;

namespace FileManager.Persistence.Tests.Repositories;

[TestFixture]
public class DepartmentRepositoryTests
{
    private static MsSqlContainer _msSqlContainer;
    private static ApplicationDbContext _sharedDbContext;

    private IDbContextTransaction _transaction;
    private ILogger<DepartmentRepository> _mockLogger;
    private DepartmentRepository _sut;

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
        _mockLogger = Substitute.For<ILogger<DepartmentRepository>>();
        _sut = new DepartmentRepository(_sharedDbContext, _mockLogger);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _sharedDbContext.ChangeTracker.Clear();
    }

    [Test]
    public async Task ExecuteAsync_WithNoFilter_ReturnsAllDepartments()
    {
        // Arrange
        await SeedAsync(new Department { Name = "Alpha" }, new Department { Name = "Beta" }, new Department { Name = "Gamma" });
        DepartmentSpecification specification = new(null, 1, 10);

        // Act
        QueryResponse<Department> result = await _sut.ExecuteAsync(specification, CancellationToken.None);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task ExecuteAsync_WithNameSearch_ReturnsMatchingDepartments()
    {
        // Arrange
        await SeedAsync(new Department { Name = "Engineering" }, new Department { Name = "Finance" }, new Department { Name = "Engine Room" });
        DepartmentSpecification specification = new("Engin", 1, 10);

        // Act
        QueryResponse<Department> result = await _sut.ExecuteAsync(specification, CancellationToken.None);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.Items.All(d => d.Name.Contains("Engin")), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithPaging_ReturnsCorrectPage()
    {
        // Arrange — seed 5 departments; ordered by name: Alpha, Beta, Delta, Epsilon, Gamma
        await SeedAsync(
            new Department { Name = "Alpha" },
            new Department { Name = "Beta" },
            new Department { Name = "Gamma" },
            new Department { Name = "Delta" },
            new Department { Name = "Epsilon" });
        DepartmentSpecification specification = new(null, 2, 2);

        // Act
        QueryResponse<Department> result = await _sut.ExecuteAsync(specification, CancellationToken.None);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.Items.First().Name, Is.EqualTo("Delta"));
        Assert.That(result.Items.Last().Name, Is.EqualTo("Epsilon"));
    }

    [Test]
    public async Task ExecuteAsync_ReturnsCorrectTotalItems()
    {
        // Arrange
        await SeedAsync(new Department { Name = "Alpha" }, new Department { Name = "Beta" }, new Department { Name = "Gamma" });
        DepartmentSpecification specification = new(null, 1, 2);

        // Act
        QueryResponse<Department> result = await _sut.ExecuteAsync(specification, CancellationToken.None);

        // Assert
        Assert.That(result.TotalItems, Is.EqualTo(3));
        Assert.That(result.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public void ExecuteAsync_WhenExceptionOccurs_Rethrows()
    {
        // Arrange
        DepartmentRepository sut = CreateSutWithDisposedContext();
        DepartmentSpecification specification = new(null, 1, 10);

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.ExecuteAsync(specification, CancellationToken.None));
    }

    [Test]
    public void ExecuteAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        ILogger<DepartmentRepository> mockLogger = Substitute.For<ILogger<DepartmentRepository>>();
        DepartmentRepository sut = CreateSutWithDisposedContext(mockLogger);
        DepartmentSpecification specification = new(null, 1, 10);

        // Act
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            sut.ExecuteAsync(specification, CancellationToken.None));

        // Assert
        AssertLoggedError(mockLogger);
    }

    private async Task SeedAsync(params Department[] departments)
    {
        _sharedDbContext.Departments.AddRange(departments);
        await _sharedDbContext.SaveChangesAsync();
        _sharedDbContext.ChangeTracker.Clear();
    }

    private DepartmentRepository CreateSutWithDisposedContext(ILogger<DepartmentRepository>? logger = null)
    {
        DbContextOptions options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .Options;
        ApplicationDbContext disposedContext = new(options);
        disposedContext.Dispose();
        return new DepartmentRepository(disposedContext, logger ?? _mockLogger);
    }

    private static void AssertLoggedError(ILogger<DepartmentRepository> logger)
    {
        bool wasLogged = logger.ReceivedCalls()
            .Any(c => c.GetMethodInfo().Name == "Log"
                   && c.GetArguments()[0] is LogLevel level
                   && level == LogLevel.Error
                   && c.GetArguments()[3] is Exception);
        Assert.That(wasLogged, Is.True);
    }
}
