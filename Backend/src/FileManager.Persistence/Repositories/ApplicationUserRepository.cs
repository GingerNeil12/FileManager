using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileManager.Persistence.Repositories;

public class ApplicationUserRepository(
    ApplicationDbContext context,
    ILogger<ApplicationUserRepository> logger
) : IApplicationUserRepository
{
    public async Task<Result<ApplicationUser, Error>> GetByAsync(string externalProviderId, CancellationToken ct)
    {
        try
        {
            ApplicationUser? user = await context
                .Users
                .FirstOrDefaultAsync(x => x.ExternalProviderId == externalProviderId, ct);

            return user is null
                ? new NotFoundError(typeof(ApplicationUser), externalProviderId)
                : user;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Unable to get user with EPI: {epi}.", externalProviderId);
            throw;
        }
    }

    public async Task SaveAsync(ApplicationUser user, CancellationToken ct)
    {
        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync(ct);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Unable to add new user with EPI: {epi}.", user.ExternalProviderId);
            throw;
        }
    }
}