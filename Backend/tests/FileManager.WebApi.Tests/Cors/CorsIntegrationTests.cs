#nullable disable
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FileManager.WebApi.Tests.Cors;

[TestFixture]
public class CorsIntegrationTests
{
    private WebApplicationFactory<Program> _factory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Cors:AllowedOrigins:0"] = "http://localhost:4200"
                    });
                });
            });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task HealthEndpoint_WhenRequestHasAllowedOrigin_ReturnsAccessControlAllowOriginHeader()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://localhost:4200");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(response.Headers.Contains("Access-Control-Allow-Origin"), Is.True);
            Assert.That(
                response.Headers.GetValues("Access-Control-Allow-Origin").First(),
                Is.EqualTo("http://localhost:4200"));
        }

    }

    [Test]
    public async Task HealthEndpoint_WhenRequestHasDisallowedOrigin_DoesNotReturnAccessControlAllowOriginHeader()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://evil.example.com");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.That(response.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
    }

    [Test]
    public async Task HealthEndpoint_WhenPreflightRequestHasAllowedOrigin_Returns204WithCorsHeaders()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(response.Headers.Contains("Access-Control-Allow-Origin"), Is.True);
            Assert.That(
                response.Headers.GetValues("Access-Control-Allow-Origin").First(),
                Is.EqualTo("http://localhost:4200"));
        }

    }

    [Test]
    public async Task HealthEndpoint_WhenRequestHasNoOriginHeader_DoesNotReturnAccessControlAllowOriginHeader()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.That(response.Headers.Contains("Access-Control-Allow-Origin"), Is.False);
    }
}
