using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetRolePermissions;

public sealed class GetRolePermissionsQueryValidator : AbstractValidator<GetRolePermissionsQuery>
{
    public GetRolePermissionsQueryValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("معرف الدور (RoleId) مطلوب ولا يمكن أن يكون فارغاً.");
    }
}