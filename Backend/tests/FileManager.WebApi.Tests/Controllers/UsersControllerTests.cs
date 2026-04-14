#nullable disable

using FileManager.Application.Workflows.CreateUser;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;
using FileManager.WebApi.Controllers;
using FileManager.WebApi.DTOs.Common;
using FileManager.WebApi.DTOs.Users;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

namespace FileManager.WebApi.Tests.Controllers;

[TestFixture]
public class UsersControllerTests
{
    private ICreateUserService _mockCreateUserService;
    private UsersController _sut;

    [SetUp]
    public void SetUp()
    {
        _mockCreateUserService = Substitute.For<ICreateUserService>();

        _sut = new UsersController(_mockCreateUserService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task CreateAsync_WhenServiceReturnsSuccess_Returns201Created()
    {
        // Arrange
        Guid newUserId = Guid.NewGuid();
        _mockCreateUserService
            .CreateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(newUserId);

        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        IActionResult result = await _sut.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task CreateAsync_WhenServiceReturnsSuccess_ResponseBodyContainsNewUserId()
    {
        // Arrange
        Guid newUserId = Guid.NewGuid();
        _mockCreateUserService
            .CreateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(newUserId);

        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        IActionResult result = await _sut.CreateAsync(dto, CancellationToken.None);

        // Assert
        var createdResult = (CreatedAtActionResult)result;
        var body = (IdResponseDto<Guid>)createdResult.Value;
        Assert.That(body.Id, Is.EqualTo(newUserId));
    }

    [Test]
    public async Task CreateAsync_WhenServiceReturnsSuccess_CallsServiceWithMappedRequest()
    {
        // Arrange
        Guid newUserId = Guid.NewGuid();
        _mockCreateUserService
            .CreateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(newUserId);

        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        await _sut.CreateAsync(dto, CancellationToken.None);

        // Assert
        await _mockCreateUserService.Received(1).CreateAsync(
            Arg.Is<CreateUserRequest>(r =>
                r.Email == dto.Email &&
                r.GivenName == dto.GivenName &&
                r.FamilyName == dto.FamilyName &&
                r.Role == dto.Role &&
                r.DepartmentId == dto.DepartmentId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateAsync_WhenServiceReturnsValidationError_Returns400()
    {
        // Arrange
        _mockCreateUserService
            .CreateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationError(new Dictionary<string, string[]>
            {
                { "Email", ["Email is required."] }
            }));

        var dto = new CreateUserDto("", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        IActionResult result = await _sut.CreateAsync(dto, CancellationToken.None);

        // Assert
        var statusResult = (ObjectResult)result;
        Assert.That(statusResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task CreateAsync_WhenServiceReturnsForbiddenError_Returns403()
    {
        // Arrange
        _mockCreateUserService
            .CreateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ForbiddenError());

        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.InternalUser, null);

        // Act
        IActionResult result = await _sut.CreateAsync(dto, CancellationToken.None);

        // Assert
        var statusResult = (ObjectResult)result;
        Assert.That(statusResult.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
    }

    [Test]
    public async Task CreateAsync_WhenServiceReturnsExternalServiceError_Returns500()
    {
        // Arrange
        _mockCreateUserService
            .CreateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ExternalServiceError("Auth0 unavailable."));

        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        IActionResult result = await _sut.CreateAsync(dto, CancellationToken.None);

        // Assert
        var statusResult = (ObjectResult)result;
        Assert.That(statusResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }
}
