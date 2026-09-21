using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.UpdateRolePermissions;
    public sealed class UpdateRolePermissionsCommandHandler(
        IPermissionManagementService permissionManagementService)
        : ICommandHandler<UpdateRolePermissionsCommand>
{
    public async Task<Result> Handle(
        UpdateRolePermissionsCommand request,
        CancellationToken cancellationToken)
    {
        return await permissionManagementService.UpdateRolePermissionsAsync(
            request.RoleId,
            request.PermissionIds,
            cancellationToken);
    }
}