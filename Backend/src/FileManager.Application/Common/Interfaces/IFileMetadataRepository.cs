using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;

namespace FileManager.Application.Common.Interfaces;

public interface IFileMetadataRepository
{
    Task SaveAsync(FileMetadata metadata, CancellationToken ct);
    Task<Result<FileMetadata, Error>> GetByAsync(Guid id, CancellationToken ct);
}