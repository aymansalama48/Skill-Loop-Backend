using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.RemovePermissionFromRole;

[AuthenticatedOnly]
public sealed record RemovePermissionFromRoleCommand(
    Guid RoleId,
    Guid PermissionId) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => [
        "Roles:AllWithPermissions",
        $"Roles:{RoleId}:Permissions"
    ];
}