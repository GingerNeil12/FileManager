namespace FileManager.Application.Workflows.GetFilteredDepartments;

public record GetFilteredDepartmentsRequest(
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    bool? OrderAscending
);