using System.Net.Http.Headers;
using System.Net.Mime;
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
        logger.LogError(exception, "Unhandled exception.");
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unhandled error occured. Try again later.",
            Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.6.1"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;
        await httpContext.Response.WriteAsJsonAsync(problemDetails);
        return true;
    }
}