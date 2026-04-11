using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;

namespace FileManager.Application.Common.Interfaces;

public interface IApplicationUserRepository
{
    Task SaveAsync(ApplicationUser user, CancellationToken ct);
    Task<Result<ApplicationUser, Error>> GetByAsync(string externalProviderId, CancellationToken ct);
    Task UpdateAsync(ApplicationUser user, CancellationToken ct);
}