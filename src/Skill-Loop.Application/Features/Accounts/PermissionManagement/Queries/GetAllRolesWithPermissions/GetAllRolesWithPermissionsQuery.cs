using Skill_Loop.Domain.Constants;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization.Models;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetAllRolesWithPermissions;

[Permission(Permissions.Access.RolesManage)]
public sealed record GetAllRolesWithPermissionsQuery() : ICacheableQuery<List<RoleWithPermissionsDto>> //[cite: 26]
{
    public string CacheKey => "Roles:AllWithPermissions"; //[cite: 26]

    // 👇 التعديل: إطالة مدة الكاش
    public TimeSpan? SlidingExpiration => TimeSpan.FromHours(12);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(24);
}
