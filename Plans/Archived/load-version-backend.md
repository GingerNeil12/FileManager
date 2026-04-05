# Loading the backend version

We would like to be able to load the version information and health status of the backend and display on the frontend. We would like this to be viewable on the same page as the Frontend's version information `/version`.

## Requirements

- Ability to see the name, version and health status of the Backend on the Frontends `/version` page.
- Tests to cover new functionality.
- Graceful error handling when the backend is unreachable.
  - This could take the form of a toast message highlighted with appropriate colour scheme to indicate error.
- If the Backend returns Healthy the UI should reflect this by highlighting the status text in green.
  - If it returns any other status it should highlight the text in red.
- This should also refactor the UI to be more extensible. Incase there is more backend services created in the future so we can display the versioning and status of them as well.
  - This could take the form of a shared model in which Name and Version are required but the health status is not. The component could load this into a list and the UI loop through the list of models to display. Conditionally render the status property. This will cover the UI not having its own health check ability but other services will.
- Paths for the routing exported from a separate `.ts` file. Do not have the path for the URL be headcoded in the respective service component.

---

## Implementation Notes

### Decisions made during implementation

- **Backend unchanged**: No backend changes were required. The frontend calls the two existing endpoints (`GET /api/version` and `GET /health`) in parallel using `forkJoin` and merges the results into a `ServiceInfo` object.
- **Toast position**: Changed from bottom-right to top-right per user preference.
- **Health status comparison**: Made case-insensitive (`.toLowerCase() === 'healthy'`) to handle any future casing variations from backend services. Comparison extracted to an `isHealthy(status)` method on the component to avoid magic strings in the template.
- **`apiBaseUrl` in environment**: Production environment uses `http://localhost:8080` to match Docker Compose port mapping (`backend` service mapped to host port 8080). Development uses `http://localhost:5115` (dotnet run default).
- **Memory leak fix**: Subscription in `ngOnInit` uses `takeUntilDestroyed(this.destroyRef)` to clean up on component destruction.
- **Non-null assertion `!`**: `errorMessage()!` is required in the template because Angular's template type checker does not narrow signal return types inside `@if` control flow.

---

## Files Summary

| Action | Path |
|--------|------|
| Modified | `Frontend/src/environments/environment.ts` |
| Modified | `Frontend/src/environments/environment.development.ts` |
| Created | `Frontend/src/app/core/api-paths.ts` |
| Created | `Frontend/src/app/core/models/service-info.model.ts` |
| Created | `Frontend/src/app/core/services/backend-version.service.ts` |
| Created | `Frontend/src/app/core/services/backend-version.service.spec.ts` |
| Created | `Frontend/src/app/core/components/toast/toast.component.ts` |
| Created | `Frontend/src/app/core/components/toast/toast.component.html` |
| Created | `Frontend/src/app/core/components/toast/toast.component.spec.ts` |
| Modified | `Frontend/src/app/app.config.ts` |
| Modified | `Frontend/src/app/pages/version/version.component.ts` |
| Modified | `Frontend/src/app/pages/version/version.component.html` |
| Modified | `Frontend/src/app/pages/version/version.component.spec.ts` |
