using FileManager.Application.Common.Interfaces;
using FileManager.Application.Common.Models;
using FileManager.Application.Common.Models.Specifications;
using FileManager.Domain.Models;
using FileManager.Persistence.Specifications;

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

    public async Task<QueryResponse<Department>> ExecuteAsync(DepartmentSpecification specification, CancellationToken ct)
    {
        try
        {
            IQueryable<Department> query = SpecificationEvaluator<Department>
                .GetQuery(context.Departments.AsNoTracking(), specification);

            int totalItems = await query.CountAsync(ct);

            int skip = (specification.PageNumber - 1) * specification.PageSize;

            List<Department> items = await query
                .Skip(skip)
                .Take(specification.PageSize)
                .ToListAsync(ct);

            return new QueryResponse<Department>(
                specification.PageSize,
                specification.PageNumber,
                totalItems,
                items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to execute department specification query.");
            throw;
        }
    }
}
