namespace FileManager.WebApi.DTOs.FileManagements;

public record MetadataSummaryDto(
    Guid MetadataId,
    string OriginalName,
    DateTime UploadedOn,
    DateTime? RemovedOn,
    MetadataOwnerDto UploadedBy,
    int AssigneeCount,
    int DepartmentCount
);