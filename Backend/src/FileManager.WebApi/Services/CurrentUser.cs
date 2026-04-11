using System.Security.Claims;

using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common.Enums;

namespace FileManager.WebApi.Services;

public class CurrentUser : ICurrentUser
{
    private const string CLAIMS_PREFIX = "http://localhost/";

    private readonly string _email;
    private readonly string _name;
    private readonly string _externalProviderId;
    private readonly UserRoles _role;
    private readonly Guid _userId;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        HttpContext context = accessor.HttpContext!;
        ClaimsPrincipal user = context.User;

        _email = user.Claims.First(x => x.Type == $"{CLAIMS_PREFIX}email").Value;
        _name = user.Claims.First(x => x.Type == $"{CLAIMS_PREFIX}name").Value;
        _externalProviderId = user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        string role = user.Claims.First(x => x.Type == $"{CLAIMS_PREFIX}roles").Value;
        _role = Enum.Parse<UserRoles>(role, true);
        _userId = (Guid)context.Items[ApplicationConstants.CURRENT_USER_ID]!;
    }

    public string GetEmail() => _email;
    public string GetName() => _name;
    public UserRoles GetRole() => _role;
    public Guid GetUserId() => _userId;
    public bool IsInRole(UserRoles role) => _role == role;
    public string GetExternalProviderId() => _externalProviderId;
}
