using FileManager.Application.Common.Interfaces;
using FileManager.Application.Common.Models;
using FileManager.Application.Common.Models.Specifications;
using FileManager.Domain.Models;

using Microsoft.Extensions.Logging;

namespace FileManager.Application.Workflows.GetFilteredDepartments;

public class GetFilteredDepartmentsService(
    IDepartmentRepository repository,
    ILogger<GetFilteredDepartmentsService> logger
) : IGetFilteredDepartmentsService
{
    public async Task<GetFilteredDepartmentsResponse> GetFileredDepartmentsAsync(
        GetFilteredDepartmentsRequest request,
         CancellationToken ct
    )
    {
        try
        {
            var specification = new DepartmentSpecification(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                request.OrderAscending
            );

            QueryResponse<Department> result = await repository.ExecuteAsync(specification, ct);

            return new GetFilteredDepartmentsResponse(
                result.PageNumber,
                result.PageSize,
                result.TotalItems,
                result.Items
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to get filtered departments.");
            throw;
        }
    }
}