using FileManager.Domain.Common;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;

namespace FileManager.Application.Common.Interfaces;

public interface IAuth0ManagementClient
{
    Task<Result<string, Error>> CreateUserAsync(
        string givenName,
        string familyName,
        string email,
        UserRoles role,
        CancellationToken ct);
}
