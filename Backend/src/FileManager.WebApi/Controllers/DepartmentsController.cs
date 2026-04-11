using FileManager.WebApi.DTOs.Common;
using FileManager.WebApi.DTOs.Departments;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ApplicationControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponseDto<DepartmentSummaryDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult GetFilteredAsync(QueryfilterDto queryfilterDto, CancellationToken ct)
    {
        return Ok();
    }

    [HttpGet]
    [Route("{id:int}")]
    [Authorize(Policy = ApplicationConstants.INTERNAL_ONLY_POLICY)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentProfileDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult GetByIdAsync([FromRoute] int id, CancellationToken ct)
    {
        return Ok();
    }
}