using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.AssignPermissionToRole;

[AuthenticatedOnly]
public sealed record AssignPermissionToRoleCommand(
    Guid RoleId,
    Guid PermissionId) : ICommand, ICacheInvalidatorCommand
{
    // نمسح الكاش الخاص بكل الأدوار، والكاش الخاص بهذا الدور تحديداً
    public IReadOnlyCollection<string> CacheKeys => [
        "Roles:AllWithPermissions",
        $"Roles:{RoleId}:Permissions"
    ];
}