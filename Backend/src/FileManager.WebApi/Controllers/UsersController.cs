using FileManager.Application.Workflows.CreateUser;
using FileManager.WebApi.DTOs.Common;
using FileManager.WebApi.DTOs.Users;
using FileManager.WebApi.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.WebApi.Controllers;

[ApiController]
[Authorize(Policy = ApplicationConstants.INTERNAL_ONLY_POLICY)]
[Route("api/[controller]")]
public class UsersController(ICreateUserService createUserService) : ApplicationControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponseDto<UserSummaryDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult GetFilteredAsync(QueryfilterDto queryFilterDto, CancellationToken ct)
    {
        return Ok();
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult GetByIdAsync([FromRoute] Guid id, CancellationToken ct)
    {
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IdResponseDto<Guid>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        CreateUserRequest request = dto.ToRequest();
        var result = await createUserService.CreateAsync(request, ct);

        if (!result.IsSuccess)
        {
            return GetResultFromError(result.Error!);
        }

        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value }, new IdResponseDto<Guid>(result.Value!));
    }

    [HttpPut]
    [Route("{id:guid}")]
    [Authorize(Policy = ApplicationConstants.ADMIN_ONLY_POLICY)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IdResponseDto<Guid>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public IActionResult UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateUserDto dto,
        CancellationToken ct)
    {
        return Ok();
    }
}