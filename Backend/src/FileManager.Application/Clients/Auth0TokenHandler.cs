using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using FileManager.Application.Options;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileManager.Application.Clients;

public class Auth0TokenHandler(
    IHttpClientFactory httpClientFactory,
    IOptions<Auth0ManagementOptions> options,
    IMemoryCache cache,
    ILogger<Auth0TokenHandler> logger) : DelegatingHandler
{
    private const string TOKEN_CACHE_KEY = "auth0_mgmt_token";
    private const string TOKEN_HTTP_CLIENT_NAME = "auth0-token";

    private readonly Auth0ManagementOptions _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        string? token = await GetTokenAsync(ct);

        if (token is null)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }

    private async Task<string?> GetTokenAsync(CancellationToken ct)
    {
        if (cache.TryGetValue(TOKEN_CACHE_KEY, out string? cachedToken) && cachedToken is not null)
        {
            logger.LogDebug("Auth0 management token served from cache.");
            return cachedToken;
        }

        HttpClient tokenClient = httpClientFactory.CreateClient(TOKEN_HTTP_CLIENT_NAME);

        using HttpResponseMessage response = await tokenClient.PostAsJsonAsync(
            "/oauth/token",
            new
            {
                grant_type = "client_credentials",
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret,
                audience = _options.Audience
            },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to obtain Auth0 management token. Status: {StatusCode}.", (int)response.StatusCode);
            return null;
        }

        TokenResponse? tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        if (tokenResponse?.AccessToken is null)
        {
            logger.LogError("Auth0 token response was empty or malformed.");
            return null;
        }

        TimeSpan ttl = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 30);
        cache.Set(TOKEN_CACHE_KEY, tokenResponse.AccessToken, ttl);

        logger.LogDebug("Auth0 management token fetched and cached for {Seconds}s.", (int)ttl.TotalSeconds);
        return tokenResponse.AccessToken;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
