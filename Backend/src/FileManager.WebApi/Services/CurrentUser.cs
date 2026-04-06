using System.Security.Claims;

using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common.Enums;

namespace FileManager.WebApi.Services;

public class CurrentUser : ICurrentUser
{
    private readonly string _email;
    private readonly string _name;
    private readonly string _userId;
    private readonly UserRoles _role;
    private static readonly string _claimsPrefix = "http://localhost/";

    public CurrentUser(IHttpContextAccessor accessor, ILogger<CurrentUser> logger)
    {
        ClaimsPrincipal user = accessor.HttpContext?.User ?? new ClaimsPrincipal();
        _email = user.Claims.First(x => x.Type == $"{_claimsPrefix}email").Value;
        _name = user.Claims.First(x => x.Type == $"{_claimsPrefix}name").Value;
        _userId = user.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        string role = user.Claims.First(x => x.Type == $"{_claimsPrefix}roles").Value;
        _role = Enum.Parse<UserRoles>(role, true);
    }

    public string GetEmail() => _email;
    public string GetName() => _name;
    public UserRoles GetRole() => _role;
    public string GetUserId() => _userId;
    public bool IsInRole(UserRoles role) => _role == role;
}