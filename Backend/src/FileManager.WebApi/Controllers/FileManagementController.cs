using FileManager.WebApi.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileManagementController : ApplicationControllerBase
{
    [HttpPost]
    public IActionResult SaveUploadAsync()
    {
        return Ok();
    }

    [HttpGet]
    [Route("{id:guid}/download")]
    public IActionResult DownloadAsync([FromRoute] Guid id, CancellationToken ct)
    {
        return Ok();
    }

    [HttpGet]
    [Route("{id:guid}/metadata")]
    public IActionResult GetMetadataAsync([FromRoute] Guid id, CancellationToken ct)
    {
        return Ok();
    }

    [HttpGet]
    public IActionResult GetFilteredAsync(QueryfilterDto queryfilterDto, CancellationToken ct)
    {
        return Ok();
    }

    [HttpDelete]
    [Route("{id:guid}")]
    public IActionResult DeleteAsync([FromRoute] Guid id, CancellationToken ct)
    {
        return NoContent();
    }
}