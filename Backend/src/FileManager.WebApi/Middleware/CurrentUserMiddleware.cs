using System.Security.Claims;

using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;
using FileManager.WebApi.Exceptions;

namespace FileManager.WebApi.Middleware;

public class CurrentUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IApplicationUserRepository applicationUserRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string externalProviderId = context.User.Claims
                .First(x => x.Type == ClaimTypes.NameIdentifier).Value;

            Result<ApplicationUser, Error> userResult = await applicationUserRepository
                .GetByAsync(externalProviderId, context.RequestAborted);

            if (!userResult.IsSuccess)
            {
                throw new CurrentUserNotFoundException(externalProviderId);
            }

            if (!userResult.Value!.IsActive)
            {
                throw new UserBlockedException(externalProviderId);
            }

            context.Items[ApplicationConstants.CURRENT_USER_ID] = userResult.Value!.Id;
        }

        await next(context);
    }
}
