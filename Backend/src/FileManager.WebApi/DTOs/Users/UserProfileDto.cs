using FileManager.Domain.Common.Enums;

namespace FileManager.WebApi.DTOs.Users;

public record UserProfileDto(
    Guid UserId,
    string Email,
    string GivenName,
    string FamilyName,
    UserRoles Role,
    bool IsActive,
    DateTime CreatedOn,
    DateTime? LastLogin,
    IReadOnlyCollection<UserUploadSummaryDto> Uploads,
    IReadOnlyCollection<UserUploadSummaryDto> Assigned,
    UserDepartmentSummaryDto? Department
);

public record UserUploadSummaryDto(
    Guid UploadId,
    string Name,
    DateTime UploadedOn
);

public record UserDepartmentSummaryDto(
    int DepartmentId,
    string Name
);