using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.AssignPermissionToRole;

public sealed class AssignPermissionToRoleCommandHandler(
   IPermissionManagementService permissionManagementService)
   : ICommandHandler<AssignPermissionToRoleCommand>
{
    public async Task<Result> Handle(
        AssignPermissionToRoleCommand request,
        CancellationToken cancellationToken)
    {
        return await permissionManagementService.AssignPermissionToRoleAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);
    }
}
