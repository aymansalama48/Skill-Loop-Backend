using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.UpdateRolePermissions;

public sealed class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("معرف الدور (RoleId) مطلوب ولا يمكن أن يكون فارغاً.");

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("قائمة الصلاحيات مطلوبة (حتى لو كانت فارغة يجب إرسال مصفوفة فارغة).")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("لا يمكن تكرار نفس معرف الصلاحية في القائمة.");
    }
}
