using FileManager.WebApi.DTOs.Common;
using FileManager.WebApi.Option;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class VersionController(IOptions<ApplicationInfo> options) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VersionDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult GetVersion()
    {
        return Ok(new VersionDto(options.Value.Name, options.Value.Version));
    }
}