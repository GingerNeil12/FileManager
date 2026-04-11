using System.Net;
using System.Text;
using System.Text.Json;

using FileManager.Application.Clients;
using FileManager.Application.Options;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using MsOptions = Microsoft.Extensions.Options.Options;

namespace FileManager.Application.Tests.Clients;

[TestFixture]
public class Auth0ManagementClientTests
{
    private const string DOMAIN = "test.auth0.com";
    private const string CLIENT_ID = "test-client-id";
    private const string CLIENT_SECRET = "test-client-secret";
    private const string MANAGEMENT_AUDIENCE = "https://test.auth0.com/api/v2/";
    private const string CONNECTION = "Username-Password-Authentication";
    private const string TEST_USER_ID = "auth0|test-user-123";
    private const string TEST_ROLE_ID = "rol_test-role-id";
    private const string TEST_ACCESS_TOKEN = "test-access-token";

    private FakeHttpMessageHandler _handler = null!;
    private IMemoryCache _cache = null!;
    private Auth0ManagementClient _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _handler = new FakeHttpMessageHandler();
        var httpClient = new HttpClient(_handler);
        _cache = new MemoryCache(new MemoryCacheOptions());

        IOptions<Auth0ManagementOptions> options = MsOptions.Create(new Auth0ManagementOptions
        {
            Domain = DOMAIN,
            ClientId = CLIENT_ID,
            ClientSecret = CLIENT_SECRET,
            ManagementAudience = MANAGEMENT_AUDIENCE,
            Connection = CONNECTION
        });

        _sut = new Auth0ManagementClient(httpClient, options, _cache);
    }

    [TearDown]
    public void TearDown()
    {
        _handler.Dispose();
        _cache.Dispose();
    }

    [Test]
    public async Task CreateUserAsync_WhenTokenFetchFails_ReturnsExternalServiceError()
    {
        // Arrange
        _handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.Unauthorized));

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
        _handler.EnqueueResponse(BuildTokenResponse());
        _handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.BadRequest));

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
        _handler.EnqueueResponse(BuildTokenResponse());
        _handler.EnqueueResponse(BuildCreateUserResponse());
        _handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError));

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
        _handler.EnqueueResponse(BuildTokenResponse());
        _handler.EnqueueResponse(BuildCreateUserResponse());
        _handler.EnqueueResponse(BuildRolesResponse([]));

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
        _handler.EnqueueResponse(BuildTokenResponse());
        _handler.EnqueueResponse(BuildCreateUserResponse());
        _handler.EnqueueResponse(BuildRolesResponse([new RoleEntry(TEST_ROLE_ID, "InternalUser")]));
        _handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.Forbidden));

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
        _handler.EnqueueResponse(BuildTokenResponse());
        _handler.EnqueueResponse(BuildCreateUserResponse());
        _handler.EnqueueResponse(BuildRolesResponse([new RoleEntry(TEST_ROLE_ID, "InternalUser")]));
        _handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.NoContent));

        // Act
        var result = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(TEST_USER_ID));
    }

    [Test]
    public async Task CreateUserAsync_WhenCalledTwice_FetchesTokenOnce()
    {
        // Arrange — one token response for two full call sequences
        _handler.EnqueueResponse(BuildTokenResponse());
        _handler.EnqueueResponse(BuildCreateUserResponse());
        _handler.EnqueueResponse(BuildRolesResponse([new RoleEntry(TEST_ROLE_ID, "InternalUser")]));
        _handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.NoContent));
        _handler.EnqueueResponse(BuildCreateUserResponse());
        _handler.EnqueueResponse(BuildRolesResponse([new RoleEntry(TEST_ROLE_ID, "InternalUser")]));
        _handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.NoContent));

        // Act
        var firstResult = await _sut.CreateUserAsync("Jane", "Doe", "jane@example.com", UserRoles.InternalUser, CancellationToken.None);
        var secondResult = await _sut.CreateUserAsync("John", "Smith", "john@example.com", UserRoles.InternalUser, CancellationToken.None);

        // Assert — both succeed, meaning the second call reused the cached token
        Assert.That(firstResult.IsSuccess, Is.True);
        Assert.That(secondResult.IsSuccess, Is.True);
        Assert.That(_handler.RequestCount, Is.EqualTo(7));
    }

    private static HttpResponseMessage BuildTokenResponse()
    {
        var body = JsonSerializer.Serialize(new
        {
            access_token = TEST_ACCESS_TOKEN,
            expires_in = 86400
        });
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }

    private static HttpResponseMessage BuildCreateUserResponse()
    {
        var body = JsonSerializer.Serialize(new { user_id = TEST_USER_ID });
        return new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }

    private static HttpResponseMessage BuildRolesResponse(RoleEntry[] roles)
    {
        var body = JsonSerializer.Serialize(roles.Select(r => new { id = r.Id, name = r.Name }));
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }

    private sealed record RoleEntry(string Id, string Name);
}

internal class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();

    public int RequestCount { get; private set; }

    public void EnqueueResponse(HttpResponseMessage response) => _responses.Enqueue(response);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No more HTTP responses queued.");
        }

        return Task.FromResult(_responses.Dequeue());
    }
}
