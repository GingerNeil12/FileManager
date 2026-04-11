namespace FileManager.WebApi.DTOs.Users;

public record UserSummaryDto(
    Guid Id,
    string Email,
    string GivenName,
    string FamilyName,
    string Role,
    bool IsActive,
    int UploadsCount,
    int AssignedCount
);