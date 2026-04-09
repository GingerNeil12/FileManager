using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;

namespace FileManager.Application.Common.Interfaces;

public interface IBlobRepository
{
    Task UploadContentAsync(string location, Stream stream, CancellationToken ct);
    Task<Result<Stream, Error>> DownloadContentAsync(string location, CancellationToken ct);
    Task DeleteContentAsync(string location, CancellationToken ct);
}
