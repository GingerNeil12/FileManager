namespace FileManager.WebApi.DTOs.FileManagements;

public record MetadataOwnerDto(
    Guid UserId,
    string FullName,
    string Email
);
