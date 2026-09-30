using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.RemovePermissionFromRole;

public sealed class RemovePermissionFromRoleCommandValidator : AbstractValidator<RemovePermissionFromRoleCommand>
{
    public RemovePermissionFromRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("This field is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("This field is required.");
    }
}