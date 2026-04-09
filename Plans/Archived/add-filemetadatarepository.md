# Add FileMetadataRepository

## Role

You are a senior .NET engineer with experience using EntityFrameworkCore and Azure blobs for storage.

## Feature

We need to implement the FileMetadataRepository. The skeleton has already been created in the Persistence project. Each of the methods should have logging and exception handling in it.

## Deviations / Decisions

- **Interface split**: Before implementing the repository, `IFileMetadataRepository` was split into two interfaces to follow SRP:
  - `IFileMetadataRepository` — DB operations only (`SaveAsync`, `GetByAsync`)
  - `IBlobRepository` — blob operations only (`UploadContentAsync`, `DownloadContentAsync`, `DeleteContentAsync`)
  - Rationale: EF Core and Azure Blob Storage are separate infrastructure concerns; splitting simplifies testing (each can be tested with only its required container) and correctly places coordination responsibility in the application layer.
- **BlobContainerClient DI fix**: The original `AddSingleton(async () => ...)` registered `Task<BlobContainerClient>` not `BlobContainerClient`. Fixed to `AddSingleton<BlobContainerClient>(_ => { ... CreateIfNotExists(); return client; })`.
- **Concrete classes are skeletons**: Method bodies left as `throw new NotImplementedException()` — implementation deferred to a follow-up task.

## Summary

| Action | File |
|--------|------|
| Modified | `Backend/src/FileManager.Application/Common/Interfaces/IFileMetadataRepository.cs` |
| Created | `Backend/src/FileManager.Application/Common/Interfaces/IBlobRepository.cs` |
| Modified | `Backend/src/FileManager.Persistence/Repositories/FileMetadataRepository.cs` |
| Created | `Backend/src/FileManager.Persistence/Repositories/BlobRepository.cs` |
| Modified | `Backend/src/FileManager.Persistence/ServiceCollectionExtensions.cs` |
