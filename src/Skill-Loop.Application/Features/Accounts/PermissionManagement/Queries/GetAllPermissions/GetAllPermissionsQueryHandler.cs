using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization.Models;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetAllPermissions;

public sealed class GetAllPermissionsQueryHandler(
    IPermissionManagementService permissionManagementService)
    : IQueryHandler<GetAllPermissionsQuery, List<PermissionDto>>
{
    public async Task<Result<List<PermissionDto>>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        return await permissionManagementService.GetAllPermissionsAsync(cancellationToken);
    }
}