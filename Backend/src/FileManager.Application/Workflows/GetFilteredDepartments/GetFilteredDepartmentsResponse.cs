using FileManager.Domain.Models;

namespace FileManager.Application.Workflows.GetFilteredDepartments;

public record GetFilteredDepartmentsResponse(
    int PageNumber,
    int PageSize,
    int TotalDepartments,
    IReadOnlyCollection<Department> Departments 
);