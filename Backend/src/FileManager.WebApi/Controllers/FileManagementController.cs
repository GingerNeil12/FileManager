using FileManager.WebApi.DTOs.Common;
using FileManager.WebApi.DTOs.FileManagements;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileManagementController : ApplicationControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IdResponseDto<Guid>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult SaveUploadAsync([FromForm] SaveUploadDto dto, CancellationToken ct)
    {
        return Created();
    }

    [HttpGet]
    [Route("{id:guid}/download")]
    public IActionResult DownloadAsync([FromRoute] Guid id, CancellationToken ct)
    {
        return Ok();
    }

    [HttpGet]
    [Route("{id:guid}/metadata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MetadataProfileDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult GetMetadataAsync([FromRoute] Guid id, CancellationToken ct)
    {
        return Ok();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponseDto<MetadataSummaryDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult GetFilteredAsync(QueryfilterDto queryfilterDto, CancellationToken ct)
    {
        return Ok();
    }

    [HttpDelete]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult DeleteAsync([FromRoute] Guid id, CancellationToken ct)
    {
        return NoContent();
    }
}