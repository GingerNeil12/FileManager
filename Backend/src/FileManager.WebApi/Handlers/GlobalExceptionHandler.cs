using System.Net.Mime;
using FileManager.WebApi.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.WebApi.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails;

        switch (exception)
        {
            case UserBlockedException:
            case CurrentUserNotFoundException:
                logger.LogWarning(exception, "User is not found or blobked.");
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "Access is unauthorized.",
                    Type = "https://www.rfc-editor.org/rfc/rfc7235#section-3.1"
                };
                break;
            default:
                logger.LogError(exception, "Unhandled exception.");
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unhandled error occured. Try again later.",
                    Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.6.1"
                };
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: MediaTypeNames.Application.ProblemJson, cancellationToken: cancellationToken);
        return true;
    }
}