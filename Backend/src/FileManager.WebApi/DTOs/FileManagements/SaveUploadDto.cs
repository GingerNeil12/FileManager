namespace FileManager.WebApi.DTOs.FileManagements;

public record SaveUploadDto(
    bool ShouldEncrypt,
    IReadOnlyCollection<Guid> AssignTo,
    IReadOnlyCollection<int> Departments,
    IFormFile Upload
);