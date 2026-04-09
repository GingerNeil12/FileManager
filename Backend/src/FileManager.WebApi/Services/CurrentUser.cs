using System.Security.Claims;
using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;
using FileManager.WebApi.Exceptions;

namespace FileManager.WebApi.Services;

public class CurrentUser : ICurrentUser
{
    private readonly string _email;
    private readonly string _name;
    private readonly string _externalProviderId;
    private readonly UserRoles _role;
    private readonly Guid _userId;
    private const string CLAIMS_PREFIX = "http://localhost/";

    public CurrentUser(
        IHttpContextAccessor accessor,
        IApplicationUserRepository applicationUserRepository
    )
    {
        ClaimsPrincipal user = accessor.HttpContext?.User ?? new ClaimsPrincipal();
        _email = user.Claims.First(x => x.Type == $"{CLAIMS_PREFIX}email").Value;
        _name = user.Claims.First(x => x.Type == $"{CLAIMS_PREFIX}name").Value;
        _externalProviderId = user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        string role = user.Claims.First(x => x.Type == $"{CLAIMS_PREFIX}roles").Value;
        _role = Enum.Parse<UserRoles>(role, true);

        Result<ApplicationUser, Error> userResult = applicationUserRepository
            .GetByAsync(_externalProviderId, accessor.HttpContext?.RequestAborted ?? CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        if (!userResult.IsSuccess)
        {
            throw new CurrentUserNotFoundException(_externalProviderId);
        }

        _userId = userResult.Value!.Id;
    }

    public string GetEmail() => _email;
    public string GetName() => _name;
    public UserRoles GetRole() => _role;
    public Guid GetUserId() => _userId;
    public bool IsInRole(UserRoles role) => _role == role;
    public string GetExternalProviderId() => _externalProviderId;
}