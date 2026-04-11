namespace FileManager.WebApi.DTOs.Users;

public record CreateUserDto(
    string Email,
    string GivenName,
    string FamilyName,
    string Role,
    int? DepartmentId
);