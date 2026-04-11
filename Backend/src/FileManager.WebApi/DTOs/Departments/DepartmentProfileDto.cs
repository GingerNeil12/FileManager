namespace FileManager.WebApi.DTOs.Departments;

public record DepartmentProfileDto(
    int DepartmentId,
    string Name,
    IReadOnlyCollection<DepartmentMemberDto> Members
);

public record DepartmentMemberDto(
    Guid UserId,
    string FullName,
    string Email
);