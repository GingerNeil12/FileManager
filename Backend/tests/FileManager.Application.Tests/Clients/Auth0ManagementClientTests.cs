using FileManager.Application.Clients;
using FileManager.Application.Options;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;

using Microsoft.Extensions.Caching.Memory;
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
    private const string CLIENT_ID = "test-client-id";
    private const string CLIENT_SECRET = "test-client-secret";
    private const string CONNECTION = "Username-Password-Authentication";
    private const string TEST_USER_ID = "auth0|test-user-123";
    private const string TEST_ROLE_ID = "rol_test-role-id";
    private const string TEST_ACCESS_TOKEN = "test-access-token";

    private WireMockServer _wireMockServer = null!;
    private IMemoryCache _cache = null!;
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
        _cache = new MemoryCache(new MemoryCacheOptions());

        IOptions<Auth0ManagementOptions> options = MsOptions.Create(new Auth0ManagementOptions
        {
            Scheme = "http",
            Domain = _wireMockServer.Urls[0].Replace("http://", string.Empty),
            ClientId = CLIENT_ID,
            ClientSecret = CLIENT_SECRET,
            Audience = $"{_wireMockServer.Urls[0]}/api/v2/",
            Connection = CONNECTION
        });

        _sut = new Auth0ManagementClient(new HttpClient(), options, _cache, NullLogger<Auth0ManagementClient>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _cache.Dispose();
    }

    [Test]
    public async Task CreateUserAsync_WhenTokenFetchFails_ReturnsExternalServiceError()
    {
        // Arrange
        _wireMockServer
            .Given(Request.Create().WithPath("/oauth/token").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(401));

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.InstanceOf<ExternalServiceError>());
    }

    [Test]
    public async Task CreateUserAsync_WhenUserCreationFails_ReturnsExternalServiceError()
    {
        // Arrange
        StubTokenEndpoint();
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
    public async Task CreateUserAsync_WhenRoleLookupFails_ReturnsExternalServiceError()
    {
        // Arrange
        StubTokenEndpoint();
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
        StubTokenEndpoint();
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
        StubTokenEndpoint();
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
        StubTokenEndpoint();
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
        StubTokenEndpoint();
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
        StubTokenEndpoint();
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

    [Test]
    public async Task CreateUserAsync_WhenCalledTwice_FetchesTokenOnce()
    {
        // Arrange
        StubTokenEndpoint();
        StubCreateUserEndpoint();
        StubRoleLookupEndpoint();
        StubAssignRoleEndpoint();

        // Act
        var firstResult = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);
        var secondResult = await _sut.CreateUserAsync("John", "Smith", "john@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(firstResult.IsSuccess, Is.True);
        Assert.That(secondResult.IsSuccess, Is.True);

        int tokenRequests = _wireMockServer.LogEntries
            .Count(e => e.RequestMessage.Path == "/oauth/token");
        Assert.That(tokenRequests, Is.EqualTo(1));
    }

    private void StubTokenEndpoint()
    {
        _wireMockServer
            .Given(Request.Create().WithPath("/oauth/token").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { access_token = TEST_ACCESS_TOKEN, expires_in = 86400 }));
    }

    private void StubCreateUserEndpoint()
    {
        _wireMockServer
            .Given(Request.Create()
                .WithPath("/api/v2/users")
                .UsingPost()
                .WithHeader("Authorization", $"Bearer {TEST_ACCESS_TOKEN}"))
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithBodyAsJson(new { user_id = TEST_USER_ID }));
    }

    private void StubRoleLookupEndpoint()
    {
        _wireMockServer
            .Given(Request.Create()
                .WithPath("/api/v2/roles")
                .UsingGet()
                .WithHeader("Authorization", $"Bearer {TEST_ACCESS_TOKEN}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new[] { new { id = TEST_ROLE_ID, name = "InternalUser" } }));
    }

    private void StubAssignRoleEndpoint()
    {
        _wireMockServer
            .Given(Request.Create()
                .WithPath($"/api/v2/users/{TEST_USER_ID}/roles")
                .UsingPost()
                .WithHeader("Authorization", $"Bearer {TEST_ACCESS_TOKEN}"))
            .RespondWith(Response.Create().WithStatusCode(204));
    }

    private void StubConflictCreateUserEndpoint()
    {
        _wireMockServer
            .Given(Request.Create()
                .WithPath("/api/v2/users")
                .UsingPost()
                .WithHeader("Authorization", $"Bearer {TEST_ACCESS_TOKEN}"))
            .RespondWith(Response.Create().WithStatusCode(409));
    }

    private void StubUserByEmailEndpoint()
    {
        _wireMockServer
            .Given(Request.Create()
                .WithPath("/api/v2/users-by-email")
                .UsingGet()
                .WithHeader("Authorization", $"Bearer {TEST_ACCESS_TOKEN}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new[] { new { user_id = TEST_USER_ID } }));
    }
}
