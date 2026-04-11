namespace FileManager.WebApi.DTOs.FileManagements;

public record MetadataProfileDto(
    Guid MetadataId,
    string OriginalName,
    string? Description,
    DateTime UploadedOn,
    DateTime? RemovedOn,
    MetadataOwnerDto UploadedBy,
    IReadOnlyCollection<MetadataAssigneeDto> AssignedTo,
    IReadOnlyCollection<MetadataDepartmentDto> Departments
);

public record MetadataAssigneeDto(
    Guid UserId,
    string FullName,
    string Email,
    DateTime AssignedOn,
    DateTime? DownloadedOn,
    DateTime? DeletedOn
);

public record MetadataDepartmentDto(
    int DepartmentId,
    string Name
);