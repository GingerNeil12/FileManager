using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using FileManager.Application.Common.Interfaces;
using FileManager.Application.Options;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileManager.Application.Clients;

public class Auth0ManagementClient(
    HttpClient httpClient,
    IOptions<Auth0ManagementOptions> options,
    IMemoryCache cache,
    ILogger<Auth0ManagementClient> logger) : IAuth0ManagementClient
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
            logger.LogDebug("Auth0 management token served from cache.");
            return cachedToken;
        }

        var requestBody = new
        {
            grant_type = "client_credentials",
            client_id = _options.ClientId,
            client_secret = _options.ClientSecret,
            audience = _options.Audience
        };

        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{_options.Scheme}://{_options.Domain}/oauth/token",
            requestBody,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to obtain Auth0 management token. Status: {StatusCode}.", (int)response.StatusCode);
            return new ExternalServiceError("Failed to obtain Auth0 Management API token.");
        }

        TokenResponse? tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        if (tokenResponse?.AccessToken is null)
        {
            logger.LogError("Auth0 token response was empty or malformed.");
            return new ExternalServiceError("Auth0 token response was empty or malformed.");
        }

        TimeSpan ttl = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 30);
        cache.Set(TOKEN_CACHE_KEY, tokenResponse.AccessToken, ttl);

        logger.LogDebug("Auth0 management token fetched and cached for {Seconds}s.", (int)ttl.TotalSeconds);
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
            $"{_options.Scheme}://{_options.Domain}/api/v2/users",
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

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            logger.LogWarning("Auth0 user already exists for email {Email}. Fetching existing user ID.", email);
            return await GetUserByEmailAsync(token, email, ct);
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Auth0 user creation failed for email {Email}. Status: {StatusCode}.", email, (int)response.StatusCode);
            return new ExternalServiceError($"Auth0 user creation failed with status {(int)response.StatusCode}.");
        }

        CreateUserResponse? userResponse = await response.Content.ReadFromJsonAsync<CreateUserResponse>(ct);
        if (userResponse?.UserId is null)
        {
            logger.LogError("Auth0 create user response did not contain a user ID for email {Email}.", email);
            return new ExternalServiceError("Auth0 create user response did not contain a user ID.");
        }

        logger.LogInformation("Auth0 user created with ID {UserId} for email {Email}.", userResponse.UserId, email);
        return userResponse.UserId;
    }

    private async Task<Result<string, Error>> GetUserByEmailAsync(
        string token,
        string email,
        CancellationToken ct)
    {
        using HttpRequestMessage request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"{_options.Scheme}://{_options.Domain}/api/v2/users-by-email?email={Uri.EscapeDataString(email)}",
            token);

        using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Auth0 user lookup by email failed for {Email}. Status: {StatusCode}.", email, (int)response.StatusCode);
            return new ExternalServiceError($"Auth0 user lookup by email failed with status {(int)response.StatusCode}.");
        }

        CreateUserResponse[]? users = await response.Content.ReadFromJsonAsync<CreateUserResponse[]>(ct);
        string? userId = users?.FirstOrDefault()?.UserId;

        if (userId is null)
        {
            logger.LogError("Auth0 returned no user for email {Email}.", email);
            return new ExternalServiceError($"Auth0 returned no user for email '{email}'.");
        }

        logger.LogInformation("Auth0 existing user resolved to ID {UserId} for email {Email}.", userId, email);
        return userId;
    }

    private async Task<Result<string, Error>> LookupRoleIdAsync(
        string token,
        UserRoles role,
        CancellationToken ct)
    {
        string roleName = role.ToString();

        using HttpRequestMessage request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"{_options.Scheme}://{_options.Domain}/api/v2/roles?name_filter={Uri.EscapeDataString(roleName)}",
            token);

        using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Auth0 role lookup failed for role {RoleName}. Status: {StatusCode}.", roleName, (int)response.StatusCode);
            return new ExternalServiceError($"Auth0 role lookup failed with status {(int)response.StatusCode}.");
        }

        RoleEntry[]? roles = await response.Content.ReadFromJsonAsync<RoleEntry[]>(ct);
        RoleEntry? match = roles?.FirstOrDefault(r => r.Name == roleName);

        if (match?.Id is null)
        {
            logger.LogError("Auth0 role '{RoleName}' was not found.", roleName);
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
            $"{_options.Scheme}://{_options.Domain}/api/v2/users/{Uri.EscapeDataString(userId)}/roles",
            token);

        request.Content = JsonContent.Create(new { roles = new[] { roleId } });

        using HttpResponseMessage response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Auth0 role assignment failed for user {UserId} with role {RoleId}. Status: {StatusCode}.", userId, roleId, (int)response.StatusCode);
            return new ExternalServiceError($"Auth0 role assignment failed with status {(int)response.StatusCode}.");
        }

        logger.LogInformation("Auth0 role {RoleId} assigned to user {UserId}.", roleId, userId);
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
