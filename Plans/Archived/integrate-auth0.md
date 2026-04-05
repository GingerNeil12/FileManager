# Integrate with Auth0

We are looking to use Auth0 as our auth provider. We need a login page for the UI to be created that would use the Auth0 Angular packages and also the backend would need to be able to decode and authenticate the tokens provided. There is skills provided for you to guide setting all this up. The login page should link to the `Login` button on the navbar.

## Required

- Auth0 login page found when user clicks on the `Login` button on the navbar.
- Auth0 integration for allowing a user to type in email/password and get an auth token from Auth0 on success.
- Backend integrated to decode the token to authenticate a user.
- Tests
- Check your skills for the skills related to Auth0 and use them for this integration.

## Not in scope

- User management other than Login/Authentication.
- User creation via the app. That will be implemented in another ticket.

---

## Implementation Notes

### Deviations from original plan

**Backend SDK changed:** `Auth0.AspNetCore.Authentication.Api` was only available as `1.0.0-beta.4` (pre-release). Switched to stable `Microsoft.AspNetCore.Authentication.JwtBearer` (v10.0.5) with manual Auth0 configuration instead. The setup is functionally equivalent:
- `options.Authority = $"https://{domain}/"` (constructs the Auth0 JWKS endpoint)
- `options.Audience = audience` (validates the `aud` claim)

**FallbackPolicy side effect:** Setting `FallbackPolicy = RequireAuthenticatedUser` also affects `MapHealthChecks` and `MapOpenApi`. Added `.AllowAnonymous()` to both to keep them publicly accessible.

**Angular guard:** Used a custom `authGuard` functional guard (instead of `AuthGuard` class from SDK) using `combineLatest([isAuthenticated$, isLoading$])` to wait for SDK initialization before evaluating auth state — prevents flash-redirects on page refresh for already-authenticated users.

**Navbar spec structure:** "navbar element" and "brand" describe blocks were merged into the "when unauthenticated" describe block (they share identical setup and don't depend on auth state) — reduced from 4 TestBed setup configurations to 2.

**`app.spec.ts`:** Updated to provide a mock `AuthService` — the `App` component renders `NavbarComponent` which now injects `AuthService`.

---

## Files Created

| File | Purpose |
|---|---|
| `Frontend/src/app/core/guards/auth.guard.ts` | Functional auth guard — waits for SDK init before checking auth state |
| `Frontend/src/app/core/guards/auth.guard.spec.ts` | Guard tests: loading state, authenticated, unauthenticated, loading transition |
| `Backend/src/FileManager.WebApi/Option/Auth0Options.cs` | Strongly typed Auth0 config options (Domain, Audience) |
| `Backend/tests/FileManager.WebApi.Tests/Auth/AuthIntegrationTests.cs` | Integration tests: AllowAnonymous endpoints return 200, protected endpoint returns 401 |

## Files Modified

| File | Change |
|---|---|
| `Frontend/src/environments/environment.ts` | Added `auth0Domain`, `auth0ClientId`, `auth0Audience` (empty — filled by deployment) |
| `Frontend/src/environments/environment.development.ts` | Added auth0 keys with placeholder values (to be filled with real credentials) |
| `Frontend/src/app/app.config.ts` | Added `provideAuth0` + `authHttpInterceptorFn` for all `apiBaseUrl/*` requests |
| `Frontend/src/app/app.routes.ts` | Added `canActivate: [authGuard]` to version route |
| `Frontend/src/app/app.spec.ts` | Added mock `AuthService` provider |
| `Frontend/src/app/core/components/navbar/navbar.component.ts` | Injected `AuthService`, added `login()` / `logout()` methods |
| `Frontend/src/app/core/components/navbar/navbar.component.html` | Conditional Login/Logout button via `@if (auth.isAuthenticated$ \| async)` |
| `Frontend/src/app/core/components/navbar/navbar.component.spec.ts` | Restructured with unauthenticated/authenticated describe blocks; mock `AuthService` |
| `Backend/src/FileManager.WebApi/appsettings.json` | Added `Auth0` placeholder section |
| `Backend/src/FileManager.WebApi/Program.cs` | Added JWT Bearer auth, default+fallback auth policies, `UseAuthentication`/`UseAuthorization` |
| `Backend/src/FileManager.WebApi/FileManager.WebApi.csproj` | Added `Microsoft.AspNetCore.Authentication.JwtBearer` package; added `UserSecretsId` |

## Files Deleted

None.

---

## Post-implementation steps required

1. Fill in real Auth0 credentials in `Frontend/src/environments/environment.development.ts`
2. Run `dotnet user-secrets set "Auth0:Domain" "..."` and `dotnet user-secrets set "Auth0:Audience" "..."` in `Backend/src/FileManager.WebApi/`
3. Confirm Auth0 dashboard SPA application has `http://localhost:4200` in Allowed Callback URLs, Allowed Logout URLs, and Allowed Web Origins
