using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileManager.Persistence.Repositories;

public class FileMetadataRepository(
    ApplicationDbContext context,
    ILogger<FileMetadataRepository> logger
) : IFileMetadataRepository
{
    public async Task<Result<FileMetadata, Error>> GetByAsync(Guid id, CancellationToken ct)
    {
        try
        {
            FileMetadata? fileMetadata = await context
                .FileMetadata
                .Include(x => x.Assignees)
                .Include(x => x.UploadedBy)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            
            return fileMetadata is null
                ? new NotFoundError(typeof(FileMetadata), id.ToString())
                : fileMetadata;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Unable to get file with Id: {id}", id);
            throw;
        }
    }
    
    public async Task SaveAsync(FileMetadata metadata, CancellationToken ct)
    {
        try
        {
            context.FileMetadata.Add(metadata);
            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to save FileMetadata: {fileName} {location}.", metadata.OriginalName, metadata.Location);
            throw;
        }
    }
}
