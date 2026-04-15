namespace FileManager.Application.Workflows.GetFilteredDepartments;

public interface IGetFilteredDepartmentsService
{
    Task<GetFilteredDepartmentsResponse> GetFileredDepartmentsAsync(GetFilteredDepartmentsRequest request, CancellationToken ct);
}