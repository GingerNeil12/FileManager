using FileManager.Domain.Common;
using FileManager.Domain.Common.Errors;

namespace FileManager.Application.Workflows.CreateUser;

public interface ICreateUserService
{
    Task<Result<Guid, Error>> CreateAsync(CreateUserRequest request, CancellationToken ct);
}