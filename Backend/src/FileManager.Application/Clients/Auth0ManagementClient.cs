using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using FileManager.Application.Common.Interfaces;
using FileManager.Application.Options;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FileManager.Application.Clients;

public class Auth0ManagementClient(
    HttpClient httpClient,
    IOptions<Auth0ManagementOptions> options,
    IMemoryCache cache) : IAuth0ManagementClient
{
    private const string TOKEN_CACHE_KEY = "auth0_mgmt_token";

    private readonly Auth0ManagementOptions _options = options.Value;

    public async Task<Result<string, Error>> CreateUserAsync(
        string givenName,
        string familyName,
        string email,
        UserRoles role,
        CancellationToken ct)
    {
        Result<string, Error> tokenResult = await GetManagementTokenAsync(ct);
        if (!tokenResult.IsSuccess)
        {
            return tokenResult.Error!;
        }

        string token = tokenResult.Value!;

        Result<string, Error> userResult = await CreateAuth0UserAsync(token, givenName, familyName, email, ct);
        if (!userResult.IsSuccess)
        {
            return userResult.Error!;
        }

        string userId = userResult.Value!;

        Result<string, Error> roleResult = await LookupRoleIdAsync(token, role, ct);
        if (!roleResult.IsSuccess)
        {
            return roleResult.Error!;
        }

        string roleId = roleResult.Value!;

        Error? assignError = await AssignRoleAsync(token, userId, roleId, ct);
        if (assignError is not null)
        {
            return assignError;
        }

        return userId;
    }

    private async Task<Result<string, Error>> GetManagementTokenAsync(CancellationToken ct)
    {
        if (cache.TryGetValue(TOKEN_CACHE_KEY, out string? cachedToken) && cachedToken is not null)
        {
            return cachedToken;
        }

        var requestBody = new
        {
            grant_type = "client_credentials",
            client_id = _options.ClientId,
            client_secret = _options.ClientSecret,
            audience = _options.ManagementAudience
        };

        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"https://{_options.Domain}/oauth/token",
            requestBody,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ExternalServiceError("Failed to obtain Auth0 Management API token.");
        }

        TokenResponse? tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        if (tokenResponse?.AccessToken is null)
        {
            return new ExternalServiceError("Auth0 token response was empty or malformed.");
        }

        TimeSpan ttl = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 30);
        cache.Set(TOKEN_CACHE_KEY, tokenResponse.AccessToken, ttl);

        return tokenResponse.AccessToken;
    }

    private async Task<Result<string, Error>> CreateAuth0UserAsync(
        string token,
        string givenName,
        string familyName,
        string email,
        CancellationToken ct)
    {
        using HttpRequestMessage request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"https://{_options.Domain}/api/v2/users",
            token);

        request.Content = JsonContent.Create(new
        {
            given_name = givenName,
            family_name = familyName,
            email,
            connection = _options.Connection,
            password = GenerateTemporaryPassword()
        });

        using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ExternalServiceError($"Auth0 user creation failed with status {(int)response.StatusCode}.");
        }

        CreateUserResponse? userResponse = await response.Content.ReadFromJsonAsync<CreateUserResponse>(ct);
        if (userResponse?.UserId is null)
        {
            return new ExternalServiceError("Auth0 create user response did not contain a user ID.");
        }

        return userResponse.UserId;
    }

    private async Task<Result<string, Error>> LookupRoleIdAsync(
        string token,
        UserRoles role,
        CancellationToken ct)
    {
        string roleName = role.ToString();

        using HttpRequestMessage request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"https://{_options.Domain}/api/v2/roles?name_filter={Uri.EscapeDataString(roleName)}",
            token);

        using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ExternalServiceError($"Auth0 role lookup failed with status {(int)response.StatusCode}.");
        }

        RoleEntry[]? roles = await response.Content.ReadFromJsonAsync<RoleEntry[]>(ct);
        RoleEntry? match = roles?.FirstOrDefault(r => r.Name == roleName);

        if (match?.Id is null)
        {
            return new ExternalServiceError($"Auth0 role '{roleName}' was not found.");
        }

        return match.Id;
    }

    private async Task<Error?> AssignRoleAsync(
        string token,
        string userId,
        string roleId,
        CancellationToken ct)
    {
        using HttpRequestMessage request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"https://{_options.Domain}/api/v2/users/{Uri.EscapeDataString(userId)}/roles",
            token);

        request.Content = JsonContent.Create(new { roles = new[] { roleId } });

        using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ExternalServiceError($"Auth0 role assignment failed with status {(int)response.StatusCode}.");
        }

        return null;
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static string GenerateTemporaryPassword() => $"Tmp!{Guid.NewGuid():N}";

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record CreateUserResponse(
        [property: JsonPropertyName("user_id")] string UserId);

    private sealed record RoleEntry(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name);
}
