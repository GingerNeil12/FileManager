using FileManager.Application.Workflows.CreateUser;
using FileManager.WebApi.DTOs.Users;

namespace FileManager.WebApi.Extensions;

public static class CreateUserDtoExtensions
{
    public static CreateUserRequest ToRequest(this CreateUserDto dto)
        => new(dto.Email, dto.GivenName, dto.FamilyName, dto.Role, dto.DepartmentId);
}
