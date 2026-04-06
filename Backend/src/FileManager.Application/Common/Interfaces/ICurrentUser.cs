using System.Security.Claims;

using FileManager.Domain.Common.Enums;

namespace FileManager.Application.Common.Interfaces;

public interface ICurrentUser
{
    string GetUserId();
    string GetEmail();
    UserRoles GetRole();
    string GetName();
    bool IsInRole(UserRoles role);
}