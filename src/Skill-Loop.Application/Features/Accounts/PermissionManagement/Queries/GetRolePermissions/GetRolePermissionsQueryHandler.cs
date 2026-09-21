using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization.Models;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetRolePermissions;

    public sealed class GetRolePermissionsQueryHandler(
        IPermissionManagementService permissionManagementService)
        : IQueryHandler<GetRolePermissionsQuery, RoleWithPermissionsDto>
{
    public async Task<Result<RoleWithPermissionsDto>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        return await permissionManagementService.GetRolePermissionsAsync(request.RoleId, cancellationToken);
    }
}
