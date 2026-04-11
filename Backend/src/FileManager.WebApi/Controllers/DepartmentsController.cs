using FileManager.WebApi.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ApplicationControllerBase
{
    [HttpGet]
    public IActionResult GetFilteredAsync(QueryfilterDto queryfilterDto, CancellationToken ct)
    {
        return Ok();
    }

    [HttpGet]
    [Route("{id:int}")]
    public IActionResult GetByIdAsync([FromRoute] int id, CancellationToken ct)
    {
        return Ok();
    }
}