# Implement blob repository

## Feature

We need to implement the BlobRepsitory found in the Persistence project. The implemented methods should have exception handling and logging. We are aiming for quick and effecient uploads and downloads to happen.

## Context

The BlobRepository is what we'll be using for handling blob interactions with Azure Blob store. The BlobRepository will just act as the gateway for this functionality. Other functionality using it will be implemented at a later date.

## Expected outcomes

- Methods implemented on BlobRepository.
- Exception handling done in each method.
- Logging done in each method.
- Unit test coverage.

## Constraints

- Logger does not need to check for if logging is enabled.
- No custom logging code just use the logger as is.

---

## Implementation Notes

### Deviations from original plan

None — implementation matched plan exactly.

### Key decisions made during implementation

| Decision | Choice | Reason |
|---|---|---|
| Blob not found on download | Return `NotFoundError` | Consistent with existing `ApplicationUserRepository` pattern |
| Blob not found on delete | Silently succeed via `DeleteIfExistsAsync` | Idempotent — already gone is still success |
| Upload/Delete exceptions | Log then re-throw | Matches existing repository error handling pattern |
| Download stream strategy | Return direct Azure blob stream | Memory efficient, no buffering needed |
| Azurite API version mismatch | `WithCommand("--skipApiVersionCheck")` | Azure.Storage.Blobs 12.27.0 uses API v2026-02-06 which Azurite 3.35.0 doesn't support natively |

### Files Summary

| File | Action |
|---|---|
| `Backend/src/FileManager.Persistence/Repositories/BlobRepository.cs` | Modified — implemented all 3 methods |
| `Backend/tests/FileManager.Persistence.Tests/FileManager.Persistence.Tests.csproj` | Modified — added `Testcontainers.Azurite` 4.11.0 |
| `Backend/tests/FileManager.Persistence.Tests/Repositories/BlobRepositoryTests.cs` | Created — 11 tests (5 Azurite integration, 6 mock exception path) |
