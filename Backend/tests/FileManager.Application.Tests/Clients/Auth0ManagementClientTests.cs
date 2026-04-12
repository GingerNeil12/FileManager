using FileManager.Application.Clients;
using FileManager.Application.Options;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using MsOptions = Microsoft.Extensions.Options.Options;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FileManager.Application.Tests.Clients;

[TestFixture]
public class Auth0ManagementClientTests
{
    private const string CONNECTION = "Username-Password-Authentication";
    private const string TEST_USER_ID = "auth0|test-user-123";
    private const string TEST_ROLE_ID = "rol_test-role-id";

    private WireMockServer _wireMockServer = null!;
    private Auth0ManagementClient _sut = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _wireMockServer = WireMockServer.Start();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _wireMockServer.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _wireMockServer.Reset();

        IOptions<Auth0ManagementOptions> options = MsOptions.Create(new Auth0ManagementOptions
        {
            Connection = CONNECTION
        });

        _sut = new Auth0ManagementClient(
            new HttpClient { BaseAddress = new Uri(_wireMockServer.Urls[0]) },
            options,
            NullLogger<Auth0ManagementClient>.Instance);
    }

    [Test]
    public async Task CreateUserAsync_WhenUserCreationFails_ReturnsExternalServiceError()
    {
        // Arrange
        _wireMockServer
            .Given(Request.Create().WithPath("/api/v2/users").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400));

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ExternalServiceError>());
    }

    [Test]
    public async Task CreateUserAsync_WhenTokenHandlerReturnsServiceUnavailable_ReturnsExternalServiceError()
    {
        // Arrange
        _wireMockServer
            .Given(Request.Create().WithPath("/api/v2/users").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(503));

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ExternalServiceError>());
    }

    [Test]
    public async Task CreateUserAsync_WhenRoleLookupFails_ReturnsExternalServiceError()
    {
        // Arrange
        StubCreateUserEndpoint();
        _wireMockServer
            .Given(Request.Create().WithPath("/api/v2/roles").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ExternalServiceError>());
    }

    [Test]
    public async Task CreateUserAsync_WhenRoleLookupReturnsNoMatch_ReturnsExternalServiceError()
    {
        // Arrange
        StubCreateUserEndpoint();
        _wireMockServer
            .Given(Request.Create().WithPath("/api/v2/roles").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(Array.Empty<object>()));

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ExternalServiceError>());
    }

    [Test]
    public async Task CreateUserAsync_WhenRoleAssignmentFails_ReturnsExternalServiceError()
    {
        // Arrange
        StubCreateUserEndpoint();
        StubRoleLookupEndpoint();
        _wireMockServer
            .Given(Request.Create().WithPath($"/api/v2/users/{TEST_USER_ID}/roles").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(403));

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ExternalServiceError>());
    }

    [Test]
    public async Task CreateUserAsync_WhenAllCallsSucceed_ReturnsUserId()
    {
        // Arrange
        StubCreateUserEndpoint();
        StubRoleLookupEndpoint();
        StubAssignRoleEndpoint();

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(TEST_USER_ID));
    }

    [Test]
    public async Task CreateUserAsync_WhenUserAlreadyExistsInAuth0_LooksUpByEmailAndReturnsUserId()
    {
        // Arrange
        StubConflictCreateUserEndpoint();
        StubUserByEmailEndpoint();
        StubRoleLookupEndpoint();
        StubAssignRoleEndpoint();

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(TEST_USER_ID));
    }

    [Test]
    public async Task CreateUserAsync_WhenUserAlreadyExistsInAuth0AndEmailLookupFails_ReturnsExternalServiceError()
    {
        // Arrange
        StubConflictCreateUserEndpoint();
        _wireMockServer
            .Given(Request.Create().WithPath("/api/v2/users-by-email").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ExternalServiceError>());
    }

    private void StubCreateUserEndpoint()
    {
        _wireMockServer
            .Given(Request.Create().WithPath("/api/v2/users").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithBodyAsJson(new { user_id = TEST_USER_ID }));
    }

    private void StubConflictCreateUserEndpoint()
    {
        _wireMockServer
            .Given(Request.Create().WithPath("/api/v2/users").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(409));
    }

    private void StubUserByEmailEndpoint()
    {
        _wireMockServer
            .Given(Request.Create().WithPath("/api/v2/users-by-email").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new[] { new { user_id = TEST_USER_ID } }));
    }

    private void StubRoleLookupEndpoint()
    {
        _wireMockServer
            .Given(Request.Create().WithPath("/api/v2/roles").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new[] { new { id = TEST_ROLE_ID, name = "InternalUser" } }));
    }

    private void StubAssignRoleEndpoint()
    {
        _wireMockServer
            .Given(Request.Create().WithPath($"/api/v2/users/{TEST_USER_ID}/roles").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(204));
    }
}
