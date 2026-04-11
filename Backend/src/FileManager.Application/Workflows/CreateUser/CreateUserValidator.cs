using FileManager.Application.Common.Interfaces;

using FluentValidation;

namespace FileManager.Application.Workflows.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator(IDepartmentRepository departmentRepository)
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty();
        
        RuleFor(x => x.GivenName)
            .MaximumLength(512)
            .NotEmpty();

        RuleFor(x => x.FamilyName)
            .MaximumLength(512)
            .NotEmpty();

        When(x => x.DepartmentId.HasValue, () =>
        {
            RuleFor(x => x.DepartmentId)
                .MustAsync(async (id, ct) =>
                {
                    return await departmentRepository.DoesExistAsync(id!.Value, ct);
                });
        });
    }
}