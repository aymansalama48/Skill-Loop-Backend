using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.AssignPermissionToRole;

public sealed class AssignPermissionToRoleCommandValidator : AbstractValidator<AssignPermissionToRoleCommand>
{
    public AssignPermissionToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("معرف الدور (RoleId) مطلوب ولا يمكن أن يكون فارغاً.");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("معرف الصلاحية (PermissionId) مطلوب ولا يمكن أن يكون فارغاً.");
    }
}