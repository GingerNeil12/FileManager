# Configure CORS

Allow the Angular frontend to connect to the .NET backend.

## Context

| | Local Dev | Docker Compose |
|---|---|---|
| Frontend origin | `http://localhost:4200` | `http://localhost` (port 80) |
| Backend | `https://localhost:7283` | `http://localhost:8080` |

## Steps

### 1. `appsettings.json` — define empty section as the schema anchor

```json
"Cors": {
  "AllowedOrigins": []
}
```

### 2. `appsettings.Development.json` — local dev origins

```json
"Cors": {
  "AllowedOrigins": ["http://localhost:4200"]
}
```

### 3. `docker-compose.yml` — inject origin via env var on the backend service

```yaml
environment:
  - ASPNETCORE_HTTP_PORTS=8080
  - Cors__AllowedOrigins__0=http://localhost
```

ASP.NET Core's env var binding maps `Cors__AllowedOrigins__0` → `Cors.AllowedOrigins[0]` automatically.

### 4. `FileManager.WebApi/Option/CorsOptions.cs` — typed options class

Consistent with the existing `ApplicationInfo` pattern.

### 5. `Program.cs` — register and apply the policy

- Extract `corsSection` reference once; pass to both `Configure<CorsOptions>()` and `corsSection.Get<CorsOptions>()` inside `AddCors` — avoids reading the section twice.
- Policy: explicit origins only, any header, any method — no wildcard `*`
- `app.UseCors(...)` placed after `UseHttpsRedirection` and before `MapControllers`
- Added `public partial class Program { }` at the bottom to enable `WebApplicationFactory<Program>` in tests.

## Notes

- No wildcard origins — explicit list only (security rule)
- Credentials not allowed until Auth0 integration is added
- Production origins are supplied via environment variables at deploy time — no config file changes needed

## Deviations from original design

- `Configure<CorsOptions>` and `AddCors` share a single `corsSection` variable rather than calling `builder.Configuration.GetSection(...)` twice.
- Constant tests for `SectionName`/`PolicyName` were omitted from `CorsOptionsTests` — they verify literal strings against themselves and the integration tests provide sufficient regression coverage.

## Summary

| File | Change |
| --- | --- |
| `Backend/src/FileManager.WebApi/Option/CorsOptions.cs` | Created — typed options class with `SectionName`, `PolicyName`, and `AllowedOrigins` |
| `Backend/src/FileManager.WebApi/Program.cs` | Modified — CORS service registration, middleware, `public partial class Program` |
| `Backend/src/FileManager.WebApi/appsettings.json` | Modified — added empty `Cors.AllowedOrigins` section |
| `Backend/src/FileManager.WebApi/appsettings.Development.json` | Modified — added `http://localhost:4200` origin |
| `docker-compose.yml` | Modified — added `Cors__AllowedOrigins__0=http://localhost` env var |
| `Backend/tests/FileManager.WebApi.Tests/Option/CorsOptionsTests.cs` | Created — unit tests for `CorsOptions` defaults |
| `Backend/tests/FileManager.WebApi.Tests/Cors/CorsIntegrationTests.cs` | Created — integration tests for CORS policy (allowed origin, disallowed origin, preflight, no origin) |
