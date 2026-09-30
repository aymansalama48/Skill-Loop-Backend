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
            .NotEmpty().WithMessage("This field is required.");

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("This field is required.")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("Invalid value.");
    }
}
