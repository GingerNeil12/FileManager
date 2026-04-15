namespace FileManager.Application.Common.Models;

public record QueryResponse<T>(
    int PageSize,
    int PageNumber,
    int TotalItems,
    IReadOnlyCollection<T> Items
);