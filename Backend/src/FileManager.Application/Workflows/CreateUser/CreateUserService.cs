using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;

using FluentValidation;

using Microsoft.Extensions.Logging;

namespace FileManager.Application.Workflows.CreateUser;

public class CreateUserService(
    IValidator<CreateUserRequest> validator,
    IAuth0ManagementClient auth0Client,
    IApplicationUserRepository userRepository,
    ICurrentUser currentUser,
    ILogger<CreateUserService> logger) : ICreateUserService
{
    public async Task<Result<Guid, Error>> CreateAsync(
        CreateUserRequest request,
        CancellationToken ct)
    {
        logger.LogInformation("Creating user with email {Email} and role {Role}.", request.Email, request.Role);

        UserRoles callerRole = currentUser.GetRole();
        if (callerRole == UserRoles.InternalUser && request.Role != UserRoles.ExternalUser)
        {
            logger.LogWarning("InternalUser attempted to create a user with role {Role}. Only ExternalUser is allowed.", request.Role);
            return new ForbiddenError();
        }

        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Validation failed for create user request with email {Email}.", request.Email);
            return new ValidationError(validationResult.ToDictionary());
        }

        if (request.Role == UserRoles.InternalUser && request.DepartmentId is null)
        {
            logger.LogWarning("DepartmentId is required when creating an InternalUser for email {Email}.", request.Email);
            return new ValidationError(new Dictionary<string, string[]>
            {
                { nameof(request.DepartmentId), ["DepartmentId is required when creating an InternalUser."] }
            });
        }

        Result<string, Error> auth0Result = await auth0Client.CreateUserAsync(
            request.GivenName,
            request.FamilyName,
            request.Email,
            request.Role,
            ct);

        if (!auth0Result.IsSuccess)
        {
            logger.LogError("Auth0 user creation failed for email {Email}.", request.Email);
            return auth0Result.Error;
        }

        string externalProviderId = auth0Result.Value;
        logger.LogInformation("Auth0 user created with ID {ExternalProviderId} for email {Email}.", externalProviderId, request.Email);

        ApplicationUser user = ApplicationUser.Create(
            externalProviderId,
            request.Email,
            request.GivenName,
            request.FamilyName,
            request.Role,
            request.DepartmentId);

        await userRepository.SaveAsync(user, ct);
        logger.LogInformation("User {UserId} saved to database for email {Email}.", user.Id, request.Email);

        return user.Id;
    }
}
