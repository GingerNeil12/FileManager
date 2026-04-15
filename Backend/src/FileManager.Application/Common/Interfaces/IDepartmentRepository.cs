using FileManager.Application.Common.Models;
using FileManager.Application.Common.Models.Specifications;
using FileManager.Domain.Models;

namespace FileManager.Application.Common.Interfaces;

public interface IDepartmentRepository
{
    Task<bool> DoesExistAsync(int departmentId, CancellationToken ct);
    Task<QueryResponse<Department>> ExecuteAsync(DepartmentSpecification specification, CancellationToken ct);
}