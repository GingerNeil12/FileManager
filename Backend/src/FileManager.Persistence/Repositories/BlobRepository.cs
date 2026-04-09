using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

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
    public async Task UploadContentAsync(string location, Stream stream, CancellationToken ct)
    {
        try
        {
            BlobClient blobClient = blobContainerClient.GetBlobClient(location);
            await blobClient.UploadAsync(stream, overwrite: true, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to upload blob to location: {location}.", location);
            throw;
        }
    }

    public async Task<Result<Stream, Error>> DownloadContentAsync(string location, CancellationToken ct)
    {
        try
        {
            BlobClient blobClient = blobContainerClient.GetBlobClient(location);
            Response<BlobDownloadStreamingResult> response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);
            return response.Value.Content;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new NotFoundError(typeof(BlobClient), location);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to download blob from location: {location}.", location);
            throw;
        }
    }

    public async Task DeleteContentAsync(string location, CancellationToken ct)
    {
        try
        {
            BlobClient blobClient = blobContainerClient.GetBlobClient(location);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to delete blob at location: {location}.", location);
            throw;
        }
    }
}
