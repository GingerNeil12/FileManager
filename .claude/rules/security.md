---
paths:
 - "**/Controllers/**"
 - "**/Services/**
 - "**/Middleware/**"
 - "**/interceptors/**"
 - "**/Auth/**"
 - "**/Authorization/**"
 - "**/guards/**"
---

# Security

## Shared

- Validate all user input at the system boundary. Never trust request parameters, headers or query strings.
- Use parameterized queries. Never concatenate user input into SQL or shell commands.
- Authentication tokens must be short-lived. Store refresh tokens server-side only.
- Never log secrets, tokens, passwords or PII.
- Use constant-time comparison for secrets and tokens.
- Rate-limit authentication endpoints.

## C# (.NET)

**Authorization**
- Every controller and endpoint must explicitly declare `[Authorize]` or `[AllowAnonymous]`. No endpoint should have authorization status by accident.
- Use policy-based or role-based authorization for resource access. Do not rely on hiding endpoints from the UI.
- **IDOR**: every file and user resource lookup must verify ownership against the current user. `GetFile(id)` without confirming `file.OwnerId == currentUser.Id` is a critical vulnerability in a file management app.

**Mass assignment**
- Never bind request bodies directly to EF entities. Always use DTOs. Users must not be able to POST fields like `OwnerId`,`IsAmdin` or `Role` and have them applied.

**File uploads**
- Validate file type by magic bytes not extension. Extensions are user-controlled.
- Enforce maximum file size limits at the ASP.NET Core pipeline level, not just application code.
- Store uploaded files outside the web root. Never in `wwwroot/`.
- Never serve files using their original user-supplied filename. Generate a safe internal name.
- Never execute or render uploaded files server-side.

**Infrastructure**
- Never store secrets in `appsettings.json`. Use User Secrets for development, environment variables or Azure Key Vault for production.
- Enforce `UseHttpRedirection()` and `UseHsts()` in the pipeline.
- Set `CORS`,`CSP` and security headers explicitly. Do not rely on defaults.

## Angular (Typescript)

- **Never use `bypassSecurityTrust`** (`bypassSecurityTrustHtml`,`bypassSecurityTrustScript` etc) without explicit documented justification. Angular's sanitizer exists for a reason. Bypassing it is an XSS risk.
- **Route guards are not optional**: every authenticated route must be protect by `AuthGuard`. Hiding a route from the navigation menu is not authorization.
- **Token storage**: prefer HttpOnly cookies over `localStorage` or `sessionStorage`. Tokens in `localStorage` are accessible to any Javascript on the page and vulnerable to XSS.
- **No secrets in environment files**: `environment.ts` and `environment.production.ts` are bundled into the Javascript output and visible to any user. Only non-secret config belongs there (API base urls, feature flags etc.)