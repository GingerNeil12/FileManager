using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;

namespace FileManager.Application.Workflows.CreateUser;

public class CreateUserService : ICreateUserService
{
    public Task<Result<Guid, Error>> CreateAsync(
        CreateUserRequest request,
        CancellationToken ct
    )
    {
        return null;
    }
}