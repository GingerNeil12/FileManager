using FileManager.Domain.Common.Errors;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.WebApi.Controllers;

public abstract class ApplicationControllerBase : ControllerBase
{
    protected IActionResult GetResultFromError(Error error)
    {
        switch(error)
        {
            case ValidationError validationError:
                var validationProblemDetails = new ValidationProblemDetails
                {
                    Title = "Bad Request",
                    Detail = "One or more errors occured.",
                    Errors = validationError.Errors.ToDictionary(),
                    Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1"
                };
                return StatusCode(StatusCodes.Status400BadRequest, validationProblemDetails);
            case NotFoundError notFoundError:
                var notFoundProblemDetails = new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = notFoundError.Message,
                    Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4"
                };
                return StatusCode(StatusCodes.Status404NotFound, notFoundProblemDetails);
            case ConflictError conflictError:
                var conflictProblemDetails = new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = conflictError.Message,
                    Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.8"
                };
                return StatusCode(StatusCodes.Status409Conflict, conflictProblemDetails);
            case ForbiddenError forbiddenError:
                var forbiddenProblemDetails = new ProblemDetails
                {
                    Title = "Forbidden",
                    Detail = forbiddenError.Message,
                    Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.3"
                };
                return StatusCode(StatusCodes.Status403Forbidden, forbiddenProblemDetails);
            case ExternalServiceError:
            default:
                var problemDetails = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occured. try again later.",
                    Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
        }
    }
}