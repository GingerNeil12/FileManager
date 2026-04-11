namespace FileManager.WebApi.DTOs.Users;

public record UpdateUserDto(
    string Role,
    bool IsActive
);