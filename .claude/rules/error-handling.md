---
paths:
 - "**/Controllers/**"
 - "**/Services/**"
 - "**/Handlers**"
 - "**/Middleware/**"
 - "**/interceptors/**"
---

# Error Handling

## Shared

- Never swallow errors silently. Log or propogate with context about what operation failed.
- Never expose stack traces, internal paths or raw database errors in responses.
- Retry transient errors (network timeouts, rate limits) with exponential backoff. Fail fast on validation and auth errors, don't retry.
- Include correlation/request Ids in error logs when available.

## C# (.NET) - Result pattern

This project uses `Result<TValue,TError>` for error handling in service and domain code.

- **Services return `Result` never throw for expected errors**: validation failures, not found, conflicts and business rule violations are expected outcomes, not exceptions. Throw only for truly unexpected failures (Infrastructure faults, programming errors).
- **Never unwrap a `Result` without checking for success first**: always handle both the success and failure cases before accessing the value.
- **Controllers own the mapping from `Result` to HTTP**: translate error types to appropriate status codes at the controller boundary. Domain errors must not leak directly into responses. Domain errors must not leak directly into responses.
- **Use `ProblemDetails` (RFC 7807) as the HTTP error response shape**: ASP.NET Core's standard. Consistent shape: `{ type, title, status, detail, traceId}`. Do not invent custom error envelopes.
- **Register global exception middleware** to catch any unhandled exceptions before ASP.NET Cores default handler internals. Log the full exception and return a generic `ProblemDetails` 500 to the client.
- **Never ignore `Task` return values**: always `await` or explicitly handle. Unawaited tasks swallow exceptions silently (equivalent of an unhandled promise).
- **Use typed error discriminators** in `TError` - not generic strings. Callers must be able to distinguish error kinds (e.g `NotFound`,`Validation`,`Conflict`) to map them correctly.

## Angular (Typescript)

- **Services must handle `HttpClient` errors**: always inclide `catchError` in the observable pipe. Errors not caught in the service bubble up as unhandled and crash the stream.
- **Never expose raw HTTP error details to the user**: map `HttpErrorResponse` to a user-facing message in the service or a shared error handler.
- **Register a global `ErrorHandler`** to catch unexpected client-side-errors. Log them and show a fallback UI rather than letting the app silently break.
- **Use typed error responses**: define an interface matching the `ProblemDetails` shape returned by the backend so error properties are not accessed as `any`.