using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.AssignRoleToUser;

public sealed class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("This field is required.")
            .MaximumLength(50).WithMessage("Length exceeds the maximum allowed.");
    }
}