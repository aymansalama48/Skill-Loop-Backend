using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization.Models;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetAllRolesWithPermissions;

public sealed class GetAllRolesWithPermissionsQueryHandler(
    IPermissionManagementService permissionManagementService)
    : IQueryHandler<GetAllRolesWithPermissionsQuery, List<RoleWithPermissionsDto>>
{
    public async Task<Result<List<RoleWithPermissionsDto>>> Handle(
        GetAllRolesWithPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        return await permissionManagementService.GetAllRolesWithPermissionsAsync(cancellationToken);
    }
}