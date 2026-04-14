using System.Text.Json.Serialization;

using FileManager.Domain.Common.Enums;

namespace FileManager.WebApi.DTOs.Users;

public record CreateUserDto(
    string Email,
    string GivenName,
    string FamilyName,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] UserRoles Role,
    int? DepartmentId);
