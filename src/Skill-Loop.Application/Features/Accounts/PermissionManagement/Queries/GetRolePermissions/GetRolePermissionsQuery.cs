using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetRolePermissions;

public sealed record GetRolePermissionsQuery(Guid RoleId) : ICacheableQuery<RoleWithPermissionsDto> 
{
    public string CacheKey => $"Roles:{RoleId}:Permissions"; 

    // 👇 التعديل: إطالة مدة الكاش
    public TimeSpan? SlidingExpiration => TimeSpan.FromHours(12);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(24);
}