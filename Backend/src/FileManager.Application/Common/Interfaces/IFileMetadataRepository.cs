using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;

namespace FileManager.Application.Common.Interfaces;

public interface IFileMetadataRepository
{
    Task UploadContentAsync(string location, Stream stream, CancellationToken ct);
    Task<Result<Stream, Error>> DownloadContentAsync(string location, CancellationToken ct);
    Task DeleteContentAsync(string location, CancellationToken ct);
    Task SaveAsync(FileMetadata metadata, CancellationToken ct);
    Task<Result<FileMetadata, Error>> GetByAsync(Guid id, CancellationToken ct);
}