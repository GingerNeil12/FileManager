using Microsoft.AspNetCore.Mvc;

namespace FileManager.WebApi.DTOs.Common;

public class QueryfilterDto
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; set; }

    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; set; }

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; }

    [FromQuery(Name = "asc")]
    public bool? OrderAscending { get; set; }

    [FromQuery(Name = "sort")]
    public string? SortColumn { get; set; }
}