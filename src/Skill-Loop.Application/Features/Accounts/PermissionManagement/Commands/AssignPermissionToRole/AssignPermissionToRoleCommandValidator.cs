using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.AssignPermissionToRole;

public sealed class AssignPermissionToRoleCommandValidator : AbstractValidator<AssignPermissionToRoleCommand>
{
    public AssignPermissionToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("This field is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("This field is required.");
    }
}