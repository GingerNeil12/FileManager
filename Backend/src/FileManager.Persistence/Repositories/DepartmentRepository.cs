using FileManager.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileManager.Persistence.Repositories;

public class DepartmentRepository(
    ApplicationDbContext context,
    ILogger<DepartmentRepository> logger
) : IDepartmentRepository
{
    public async Task<bool> DoesExistAsync(int departmentId, CancellationToken ct)
    {
        try
        {
            return await context.Departments.AnyAsync(x => x.Id == departmentId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to check if department exists: {id}.", departmentId);
            throw;
        }
    }
}