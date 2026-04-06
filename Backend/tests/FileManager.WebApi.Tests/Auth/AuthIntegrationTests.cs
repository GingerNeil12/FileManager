#nullable disable
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.WebApi.Tests.Auth;

[TestFixture]
public class AuthIntegrationTests
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
                        ["Auth0:Domain"] = "test-tenant.us.auth0.com",
                        ["Auth0:Audience"] = "https://test-api-identifier",
                        ["Cors:AllowedOrigins:0"] = "http://localhost:4200"
                    });
                });

                host.ConfigureTestServices(services =>
                {
                    services.AddControllers()
                        .AddApplicationPart(typeof(AuthIntegrationTests).Assembly);
                });
            });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task VersionEndpoint_WithoutToken_Returns200()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/version");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task VersionEndpoint_WithInvalidToken_Returns200()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid.token.value");

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/version");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task HealthEndpoint_WithoutToken_Returns200()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/auth-test");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ProtectedEndpoint_WithInvalidToken_Returns401()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid.token.value");

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/auth-test");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}

[ApiController]
[Route("api/auth-test")]
internal sealed class AuthTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}
