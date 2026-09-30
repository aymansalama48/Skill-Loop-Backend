using Skill_Loop.Domain.Constants;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetRolePermissions;

[Permission(Permissions.Access.RolesManage)]
public sealed record GetRolePermissionsQuery(Guid RoleId) : ICacheableQuery<RoleWithPermissionsDto> 
{
    public string CacheKey => $"Roles:{RoleId}:Permissions"; 

    // ?? �������: ����� ��� �����
    public TimeSpan? SlidingExpiration => TimeSpan.FromHours(12);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(24);
}
