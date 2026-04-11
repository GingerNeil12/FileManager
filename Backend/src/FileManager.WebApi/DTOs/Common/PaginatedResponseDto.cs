namespace FileManager.WebApi.DTOs.Common;

public record PaginatedResponseDto<T>(
    int PageNumber,
    int PageSize,
    int ItemCount,
    IReadOnlyCollection<T> Items
);