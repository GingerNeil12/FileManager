#nullable disable

using FileManager.Application.Common.Interfaces;
using FileManager.Application.Workflows.CreateUser;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Models;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

namespace FileManager.Application.Tests.Workflows.CreateUser;

[TestFixture]
public class CreateUserServiceTests
{
    private IValidator<CreateUserRequest> _mockValidator;
    private IAuth0ManagementClient _mockAuth0Client;
    private IApplicationUserRepository _mockUserRepository;
    private ICurrentUser _mockCurrentUser;
    private CreateUserService _sut;

    [SetUp]
    public void SetUp()
    {
        _mockValidator = Substitute.For<IValidator<CreateUserRequest>>();
        _mockAuth0Client = Substitute.For<IAuth0ManagementClient>();
        _mockUserRepository = Substitute.For<IApplicationUserRepository>();
        _mockCurrentUser = Substitute.For<ICurrentUser>();

        _sut = new CreateUserService(
            _mockValidator,
            _mockAuth0Client,
            _mockUserRepository,
            _mockCurrentUser,
            NullLogger<CreateUserService>.Instance);
    }

    private void SetupPassingValidation()
    {
        _mockValidator
            .ValidateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private void SetupAuth0Success(string externalProviderId = "auth0|test-user-123")
    {
        _mockAuth0Client
            .CreateUserAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<UserRoles>(),
                Arg.Any<CancellationToken>())
            .Returns(externalProviderId);
    }

    // CreateAsync — validation

    [Test]
    public async Task CreateAsync_WhenValidationFails_ReturnsValidationError()
    {
        // Arrange
        var failures = new List<ValidationFailure> { new("Email", "Email is required.") };
        _mockValidator
            .ValidateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var request = new CreateUserRequest(string.Empty, "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ValidationError>());
    }

    [Test]
    public async Task CreateAsync_WhenValidationFails_DoesNotCallAuth0()
    {
        // Arrange
        var failures = new List<ValidationFailure> { new("Email", "Email is required.") };
        _mockValidator
            .ValidateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var request = new CreateUserRequest(string.Empty, "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await _mockAuth0Client.DidNotReceive().CreateUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UserRoles>(),
            Arg.Any<CancellationToken>());
    }

    // CreateAsync — role permission checks

    [Test]
    public async Task CreateAsync_WhenInternalUserCreatesInternalUser_ReturnsForbiddenError()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalUser);
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalUser, 1);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ForbiddenError>());
    }

    [Test]
    public async Task CreateAsync_WhenInternalUserCreatesInternalAdmin_ReturnsForbiddenError()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalUser);
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalAdmin, null);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ForbiddenError>());
    }

    [Test]
    public async Task CreateAsync_WhenInternalUserCreatesInternalUser_DoesNotCallAuth0()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalUser);
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalUser, 1);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await _mockAuth0Client.DidNotReceive().CreateUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UserRoles>(),
            Arg.Any<CancellationToken>());
    }

    // CreateAsync — DepartmentId business rule

    [Test]
    public async Task CreateAsync_WhenCreatingInternalUserWithoutDepartmentId_ReturnsValidationError()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalUser, null);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ValidationError>());
    }

    [Test]
    public async Task CreateAsync_WhenCreatingInternalUserWithoutDepartmentId_ErrorContainsDepartmentIdKey()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalUser, null);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        var error = (ValidationError)result.Error;
        Assert.That(error.Errors, Contains.Key(nameof(CreateUserRequest.DepartmentId)));
    }

    [Test]
    public async Task CreateAsync_WhenCreatingInternalUserWithoutDepartmentId_DoesNotCallAuth0()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalUser, null);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await _mockAuth0Client.DidNotReceive().CreateUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UserRoles>(),
            Arg.Any<CancellationToken>());
    }

    // CreateAsync — Auth0 failure

    [Test]
    public async Task CreateAsync_WhenAuth0ReturnsError_ReturnsError()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        var auth0Error = new ExternalServiceError("Auth0 failed.");
        _mockAuth0Client
            .CreateUserAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<UserRoles>(),
                Arg.Any<CancellationToken>())
            .Returns(auth0Error);

        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ExternalServiceError>());
    }

    [Test]
    public async Task CreateAsync_WhenAuth0ReturnsError_DoesNotSaveToRepository()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        var auth0Error = new ExternalServiceError("Auth0 failed.");
        _mockAuth0Client
            .CreateUserAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<UserRoles>(),
                Arg.Any<CancellationToken>())
            .Returns(auth0Error);

        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await _mockUserRepository.DidNotReceive().SaveAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Any<CancellationToken>());
    }

    // CreateAsync — happy paths

    [Test]
    public async Task CreateAsync_WhenInternalAdminCreatesExternalUser_ReturnsSuccessWithGuid()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        SetupAuth0Success();
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task CreateAsync_WhenInternalAdminCreatesInternalUserWithDepartmentId_ReturnsSuccessWithGuid()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        SetupAuth0Success();
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalUser, 42);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task CreateAsync_WhenInternalAdminCreatesInternalAdmin_ReturnsSuccessWithGuid()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        SetupAuth0Success();
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalAdmin, null);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task CreateAsync_WhenInternalUserCreatesExternalUser_ReturnsSuccessWithGuid()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalUser);
        SetupAuth0Success();
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));
    }

    // CreateAsync — side-effect verification

    [Test]
    public async Task CreateAsync_WhenSuccessful_CallsAuth0WithCorrectArguments()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        SetupAuth0Success();
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await _mockAuth0Client.Received(1).CreateUserAsync(
            Arg.Is<string>(v => v == "Jane"),
            Arg.Is<string>(v => v == "Doe"),
            Arg.Is<string>(v => v == "jane@example.com"),
            Arg.Is<UserRoles>(v => v == UserRoles.ExternalUser),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateAsync_WhenSuccessful_SavesUserWithCorrectExternalProviderId()
    {
        // Arrange
        const string externalProviderId = "auth0|test-user-123";
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        SetupAuth0Success(externalProviderId);
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await _mockUserRepository.Received(1).SaveAsync(
            Arg.Is<ApplicationUser>(u => u.ExternalProviderId == externalProviderId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateAsync_WhenSuccessful_SavesUserWithCorrectEmail()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        SetupAuth0Success();
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await _mockUserRepository.Received(1).SaveAsync(
            Arg.Is<ApplicationUser>(u => u.Email == "jane@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateAsync_WhenSuccessful_SavesUserWithCorrectRole()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        SetupAuth0Success();
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalAdmin, null);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await _mockUserRepository.Received(1).SaveAsync(
            Arg.Is<ApplicationUser>(u => u.Role == UserRoles.InternalAdmin),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateAsync_WhenCreatingInternalUserWithDepartmentId_SavesUserWithDepartmentId()
    {
        // Arrange
        SetupPassingValidation();
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        SetupAuth0Success();
        var request = new CreateUserRequest("jane@example.com", "Jane", "Doe", UserRoles.InternalUser, 42);

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        await _mockUserRepository.Received(1).SaveAsync(
            Arg.Is<ApplicationUser>(u => u.DepartmentId == 42),
            Arg.Any<CancellationToken>());
    }
}
