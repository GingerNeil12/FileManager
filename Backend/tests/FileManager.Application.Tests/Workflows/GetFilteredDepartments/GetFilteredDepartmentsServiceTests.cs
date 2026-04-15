#nullable disable

using FileManager.Application.Common.Interfaces;
using FileManager.Application.Common.Models;
using FileManager.Application.Common.Models.Specifications;
using FileManager.Application.Workflows.GetFilteredDepartments;
using FileManager.Domain.Models;

using Microsoft.Extensions.Logging;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FileManager.Application.Tests.Workflows.GetFilteredDepartments;

[TestFixture]
public class GetFilteredDepartmentsServiceTests
{
    private IDepartmentRepository _mockRepository;
    private ILogger<GetFilteredDepartmentsService> _mockLogger;
    private GetFilteredDepartmentsService _sut;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = Substitute.For<IDepartmentRepository>();
        _mockLogger = Substitute.For<ILogger<GetFilteredDepartmentsService>>();
        _sut = new GetFilteredDepartmentsService(_mockRepository, _mockLogger);
    }

    private QueryResponse<Department> BuildQueryResponse(
        int pageNumber = 1,
        int pageSize = 10,
        int totalItems = 0,
        List<Department> items = null)
    {
        return new QueryResponse<Department>(pageSize, pageNumber, totalItems, items ?? []);
    }

    private void SetupRepository(QueryResponse<Department> response)
    {
        _mockRepository
            .ExecuteAsync(Arg.Any<DepartmentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(response);
    }

    // GetFileredDepartmentsAsync — response mapping

    [Test]
    public async Task GetFileredDepartmentsAsync_WhenRepositoryReturnsData_ReturnsCorrectPageNumber()
    {
        // Arrange
        SetupRepository(BuildQueryResponse(pageNumber: 3, pageSize: 10));
        var request = new GetFilteredDepartmentsRequest(3, 10, null, null);

        // Act
        GetFilteredDepartmentsResponse result = await _sut.GetFileredDepartmentsAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.PageNumber, Is.EqualTo(3));
    }

    [Test]
    public async Task GetFileredDepartmentsAsync_WhenRepositoryReturnsData_ReturnsCorrectPageSize()
    {
        // Arrange
        SetupRepository(BuildQueryResponse(pageNumber: 1, pageSize: 25));
        var request = new GetFilteredDepartmentsRequest(1, 25, null, null);

        // Act
        GetFilteredDepartmentsResponse result = await _sut.GetFileredDepartmentsAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.PageSize, Is.EqualTo(25));
    }

    [Test]
    public async Task GetFileredDepartmentsAsync_WhenRepositoryReturnsData_ReturnsCorrectTotalDepartments()
    {
        // Arrange
        SetupRepository(BuildQueryResponse(totalItems: 42));
        var request = new GetFilteredDepartmentsRequest(1, 10, null, null);

        // Act
        GetFilteredDepartmentsResponse result = await _sut.GetFileredDepartmentsAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.TotalDepartments, Is.EqualTo(42));
    }

    [Test]
    public async Task GetFileredDepartmentsAsync_WhenRepositoryReturnsData_ReturnsCorrectDepartments()
    {
        // Arrange
        var departments = new List<Department>
        {
            new() { Id = 1, Name = "Engineering" },
            new() { Id = 2, Name = "Finance" }
        };
        SetupRepository(BuildQueryResponse(totalItems: 2, items: departments));
        var request = new GetFilteredDepartmentsRequest(1, 10, null, null);

        // Act
        GetFilteredDepartmentsResponse result = await _sut.GetFileredDepartmentsAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.Departments, Is.EqualTo(departments));
    }

    [Test]
    public async Task GetFileredDepartmentsAsync_WhenRepositoryReturnsEmptyPage_ReturnsEmptyDepartments()
    {
        // Arrange
        SetupRepository(BuildQueryResponse(totalItems: 0, items: []));
        var request = new GetFilteredDepartmentsRequest(1, 10, null, null);

        // Act
        GetFilteredDepartmentsResponse result = await _sut.GetFileredDepartmentsAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.Departments, Is.Empty);
    }

    // GetFileredDepartmentsAsync — repository call verification

    [Test]
    public async Task GetFileredDepartmentsAsync_Always_CallsRepositoryOnce()
    {
        // Arrange
        SetupRepository(BuildQueryResponse());
        var request = new GetFilteredDepartmentsRequest(1, 10, null, null);

        // Act
        await _sut.GetFileredDepartmentsAsync(request, CancellationToken.None);

        // Assert
        await _mockRepository.Received(1).ExecuteAsync(
            Arg.Any<DepartmentSpecification>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetFileredDepartmentsAsync_Always_PassesCancellationTokenToRepository()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken ct = cts.Token;
        SetupRepository(BuildQueryResponse());
        var request = new GetFilteredDepartmentsRequest(1, 10, null, null);

        // Act
        await _sut.GetFileredDepartmentsAsync(request, ct);

        // Assert
        await _mockRepository.Received(1).ExecuteAsync(
            Arg.Any<DepartmentSpecification>(),
            Arg.Is<CancellationToken>(t => t == ct));
    }

    // GetFileredDepartmentsAsync — exception handling

    [Test]
    public void GetFileredDepartmentsAsync_WhenRepositoryThrows_RethrowsException()
    {
        // Arrange
        _mockRepository
            .ExecuteAsync(Arg.Any<DepartmentSpecification>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database error."));

        var request = new GetFilteredDepartmentsRequest(1, 10, null, null);

        // Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.GetFileredDepartmentsAsync(request, CancellationToken.None));
    }

    [Test]
    public void GetFileredDepartmentsAsync_WhenRepositoryThrows_PreservesExceptionType()
    {
        // Arrange
        _mockRepository
            .ExecuteAsync(Arg.Any<DepartmentSpecification>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException("Connection timed out."));

        var request = new GetFilteredDepartmentsRequest(1, 10, null, null);

        // Assert
        Assert.ThrowsAsync<TimeoutException>(() =>
            _sut.GetFileredDepartmentsAsync(request, CancellationToken.None));
    }
}
