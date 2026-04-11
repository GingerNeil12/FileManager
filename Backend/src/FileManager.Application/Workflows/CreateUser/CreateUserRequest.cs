using FileManager.Domain.Common.Enums;

namespace FileManager.Application.Workflows.CreateUser;

public record CreateUserRequest(
    string Email,
    string GivenName,
    string FamilyName,
    UserRoles Role,
    int? DepartmentId
);