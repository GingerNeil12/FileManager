# Auth0 Management Client

## Feature

We need a client for creating users in Auth0. You should use the `auth0-aspnetcore-api` skill to help with this. This client will be in the `Application` layer and registered to the DI container. We will also need this to add a supplied role to the user as well. The GivenName, FamilyName, Email and Role will be provided for creation.

## Context

We need to be able to create a user in Auth0 via the Management API. This client will extend at a later date for other functionality but right now we just need it to be able to create a new user and return the auth0 id.

## Expected Outcome

- New Auth0ManagementClient.
  - Name can be something else.
- Method for creating a new User.
- Unit tests.
- Use of options pattern to get auth0 values out of the `IConfiguration`.
  - These will be stored in the appsettings.json.

## Constraints

- Only the create method and whatever methods are needed to implement this. No other functionality needed at this point.
- Do not pass the `IConfiguration` around for accessing env properties.
- Everything in place so I just need to add the relevant secrets for sending to Auth0.

## Considerations

- Auth0 may issue a token for when sending requests to the Management API. If it does we would need to store it locally in a cache so that a new one isn't issued for every request if one can be reused.
  - The skill will let you know if a token is provided or not.

---

## Implementation Notes

### Decisions Made During Implementation

| Decision | Chosen Approach | Reason |
| --- | --- | --- |
| Role ID resolution | Runtime lookup via `GET /api/v2/roles?name_filter={roleName}` | No config needed; Auth0 role names match `UserRoles` enum names exactly |
| Management API audience | Explicit config value (`ManagementAudience` in options) | User preference |
| Auth0 connection | Configurable (`Connection` in `Auth0ManagementOptions`) | Tenant connection names vary |
| Auth0 API error handling | Return `ExternalServiceError` via `Result<>` pattern | Consistent with codebase error handling |
| Temporary user password | `"Tmp!{Guid.NewGuid():N}"` (user must reset on first login) | Auth0 requires a password for Username-Password-Authentication connection |

### Deviations from Original Plan

- `AssignRoleAsync` returns `Task<Error?>` (null = success) rather than `Task<Result<string, Error>>` — returning `userId` from that method was misleading since the caller already holds it.
- `HttpRequestMessage` instances are disposed via `using` — flagged during simplify review as a resource leak.
- Extracted `CreateAuthorizedRequest()` private helper — the three Auth0 API call methods all shared the same `HttpRequestMessage + Authorization header` setup pattern.

---

## Files Summary

| File | Status |
| --- | --- |
| `Backend/src/FileManager.Domain/Common/Errors/Error.cs` | Modified — added `ExternalService` to `ErrorType` enum |
| `Backend/src/FileManager.Domain/Common/Errors/ExternalServiceError.cs` | Created |
| `Backend/src/FileManager.Application/Options/Auth0ManagementOptions.cs` | Created |
| `Backend/src/FileManager.Application/Common/Interfaces/IAuth0ManagementClient.cs` | Created |
| `Backend/src/FileManager.Application/Clients/Auth0ManagementClient.cs` | Created |
| `Backend/src/FileManager.Application/ServiceCollectionExtensions.cs` | Modified — added options, memory cache, typed HttpClient registration |
| `Backend/src/FileManager.Application/FileManager.Application.csproj` | Modified — added `Microsoft.Extensions.Caching.Abstractions`, `Microsoft.Extensions.Caching.Memory`, `Microsoft.Extensions.Http` |
| `Backend/src/FileManager.WebApi/appsettings.json` | Modified — added `Auth0Management` config section |
| `Backend/tests/FileManager.Application.Tests/Clients/Auth0ManagementClientTests.cs` | Created — 7 unit tests |
