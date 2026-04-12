using System.Net;

using FileManager.Application.Clients;
using FileManager.Application.Options;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using MsOptions = Microsoft.Extensions.Options.Options;

using NSubstitute;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FileManager.Application.Tests.Clients;

[TestFixture]
public class Auth0TokenHandlerTests
{
    private const string CLIENT_ID = "test-client-id";
    private const string CLIENT_SECRET = "test-client-secret";
    private const string TEST_ACCESS_TOKEN = "test-access-token";

    private WireMockServer _wireMockServer = null!;
    private IMemoryCache _cache = null!;
    private TestInnerHandler _testInnerHandler = null!;
    private HttpClient _httpClient = null!;

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
        _testInnerHandler = new TestInnerHandler();

        IHttpClientFactory mockFactory = Substitute.For<IHttpClientFactory>();
        mockFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient { BaseAddress = new Uri(_wireMockServer.Urls[0]) });

        IOptions<Auth0ManagementOptions> options = MsOptions.Create(new Auth0ManagementOptions
        {
            ClientId = CLIENT_ID,
            ClientSecret = CLIENT_SECRET,
            Audience = $"{_wireMockServer.Urls[0]}/api/v2/"
        });

        var handler = new Auth0TokenHandler(mockFactory, options, _cache, NullLogger<Auth0TokenHandler>.Instance)
        {
            InnerHandler = _testInnerHandler
        };

        _httpClient = new HttpClient(handler);
    }

    [TearDown]
    public void TearDown()
    {
        _cache.Dispose();
        _httpClient.Dispose();
        _testInnerHandler.Dispose();
    }

    [Test]
    public async Task SendAsync_WhenTokenFetchSucceeds_AddsBearerHeaderToForwardedRequest()
    {
        // Arrange
        StubTokenEndpoint();

        // Act
        await _httpClient.GetAsync("http://some-api/resource");

        // Assert
        Assert.That(_testInnerHandler.LastRequest?.Headers.Authorization?.Scheme, Is.EqualTo("Bearer"));
        Assert.That(_testInnerHandler.LastRequest?.Headers.Authorization?.Parameter, Is.EqualTo(TEST_ACCESS_TOKEN));
    }

    [Test]
    public async Task SendAsync_WhenTokenAlreadyCached_DoesNotCallTokenEndpointAgain()
    {
        // Arrange
        StubTokenEndpoint();

        // Act
        await _httpClient.GetAsync("http://some-api/resource");
        await _httpClient.GetAsync("http://some-api/resource");

        // Assert
        int tokenRequests = _wireMockServer.LogEntries.Count(e => e.RequestMessage.Path == "/oauth/token");
        Assert.That(tokenRequests, Is.EqualTo(1));
    }

    [Test]
    public async Task SendAsync_WhenTokenFetchFails_ReturnsServiceUnavailableWithoutForwardingRequest()
    {
        // Arrange
        _wireMockServer
            .Given(Request.Create().WithPath("/oauth/token").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(500));

        // Act
        HttpResponseMessage result = await _httpClient.GetAsync("http://some-api/resource");

        // Assert
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(_testInnerHandler.LastRequest, Is.Null);
    }

    private void StubTokenEndpoint()
    {
        _wireMockServer
            .Given(Request.Create().WithPath("/oauth/token").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new { access_token = TEST_ACCESS_TOKEN, expires_in = 86400 }));
    }

    private sealed class TestInnerHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
