using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.RemovePermissionFromRole;

public sealed class RemovePermissionFromRoleCommandValidator : AbstractValidator<RemovePermissionFromRoleCommand>
{
    public RemovePermissionFromRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("معرف الدور (RoleId) مطلوب ولا يمكن أن يكون فارغاً.");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("معرف الصلاحية (PermissionId) مطلوب ولا يمكن أن يكون فارغاً.");
    }
}