namespace Skill_Loop.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.AssignPermissionToRole;
using Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.RemovePermissionFromRole;
using Skill_Loop.Application.Features.Accounts.PermissionManagement.Commands.UpdateRolePermissions;
using Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetAllPermissions;
using Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetAllRolesWithPermissions;
using Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetRolePermissions;
/// <summary>
/// إدارة الصلاحيات والأدوار (Roles) في النظام
/// </summary>
[Authorize] 
[Route("api/v1/[controller]")]
public class PermissionManagementController : BaseApiController
{
    [HttpGet("permissions")]
    public async Task<IResult> GetAllPermissions(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAllPermissionsQuery(), cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("roles")]
    public async Task<IResult> GetAllRolesWithPermissions(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAllRolesWithPermissionsQuery(), cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("roles/{roleId}")]
    public async Task<IResult> GetRolePermissions(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetRolePermissionsQuery(roleId), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("roles/{roleId}/permissions/{permissionId}/assign")]
    public async Task<IResult> AssignPermissionToRole(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new AssignPermissionToRoleCommand(roleId, permissionId), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("roles/{roleId}/permissions/{permissionId}/remove")]
    public async Task<IResult> RemovePermissionFromRole(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RemovePermissionFromRoleCommand(roleId, permissionId), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("roles/{roleId}/permissions/update")]
    public async Task<IResult> UpdateRolePermissions(Guid roleId, [FromBody] List<Guid> permissionIds, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateRolePermissionsCommand(roleId, permissionIds), cancellationToken);
        return HandleResult(result);
    }
}
