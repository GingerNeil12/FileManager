using FileManager.Application.Common.Interfaces;

namespace FileManager.WebApi.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IServiceProvider services)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            ICurrentUser currentUser = services.GetRequiredService<ICurrentUser>();

            logger.LogInformation(
                "User {UserId} with role {Role} is accessing {Path}",
                currentUser.GetUserId(),
                currentUser.GetRole(),
                context.Request.Path);
        }

        await next(context);
    }
}
