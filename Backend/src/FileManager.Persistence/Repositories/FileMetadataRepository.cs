using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;

using Microsoft.Extensions.Logging;

namespace FileManager.Persistence.Repositories;

public class FileMetadataRepository(
    ApplicationDbContext context,
    ILogger<FileMetadataRepository> logger
) : IFileMetadataRepository
{
    public Task<Result<FileMetadata, Error>> GetByAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task SaveAsync(FileMetadata metadata, CancellationToken ct) => throw new NotImplementedException();
}
