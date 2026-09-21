using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.RemovePermissionFromRole;

public sealed class RemovePermissionFromRoleCommandHandler(
    IPermissionManagementService permissionManagementService)
    : ICommandHandler<RemovePermissionFromRoleCommand>
{
    public async Task<Result> Handle(
        RemovePermissionFromRoleCommand request,
        CancellationToken cancellationToken)
    {
        return await permissionManagementService.RemovePermissionFromRoleAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);
    }
}