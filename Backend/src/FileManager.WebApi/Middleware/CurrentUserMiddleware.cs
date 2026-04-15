using System.Security.Claims;

using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;
using FileManager.WebApi.Exceptions;

namespace FileManager.WebApi.Middleware;

public class CurrentUserMiddleware(RequestDelegate next, TimeProvider timeProvider)
{
    private static readonly TimeSpan _sessionThreshold = TimeSpan.FromHours(1);

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

            ApplicationUser user = userResult.Value;

            if (!user.IsActive)
            {
                throw new UserBlockedException(externalProviderId);
            }

            if (ShouldUpdateLastLogin(user, timeProvider.GetUtcNow()))
            {
                user.LoggedIn();
                await applicationUserRepository.UpdateAsync(user, context.RequestAborted);
            }

            context.Items[ApplicationConstants.CURRENT_USER_ID] = user.Id;
        }

        await next(context);
    }

    private static bool ShouldUpdateLastLogin(ApplicationUser user, DateTimeOffset now)
        => user.LastLogin is null || (now.UtcDateTime - user.LastLogin.Value) >= _sessionThreshold;
}
