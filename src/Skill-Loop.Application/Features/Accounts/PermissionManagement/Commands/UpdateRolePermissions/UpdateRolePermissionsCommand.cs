using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.UpdateRolePermissions;
[AuthenticatedOnly]
public sealed record UpdateRolePermissionsCommand(
    Guid RoleId,
    List<Guid> PermissionIds) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => [
        "Roles:AllWithPermissions",
        $"Roles:{RoleId}:Permissions"
    ];
}
