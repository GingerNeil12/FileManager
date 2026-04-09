using Azure.Storage.Blobs;

using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;

using Microsoft.Extensions.Logging;

namespace FileManager.Persistence.Repositories;

public class BlobRepository(
    BlobContainerClient blobContainerClient,
    ILogger<BlobRepository> logger
) : IBlobRepository
{
    public Task DeleteContentAsync(string location, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<Stream, Error>> DownloadContentAsync(string location, CancellationToken ct) => throw new NotImplementedException();
    public Task UploadContentAsync(string location, Stream stream, CancellationToken ct) => throw new NotImplementedException();
}
