namespace Skill_Loop.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Users;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ActivateUser;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.AssignRoleToUser;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.DeactivateUser;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.RemoveRoleFromUser;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetAllUsers;
/// <summary>
/// إدارة مستخدمي النظام بشكل عام
/// </summary>
[Route("api/v1/[controller]")]
//[Authorize] // ����� ������ ������� ���
public class UsersController : BaseApiController
{
    [HttpGet]
    public async Task<IResult> GetAllUsers(
        [FromQuery] GetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetAllUsersQuery(
            request.PageNumber,
            request.PageSize,
            request.Role,
            request.SearchTerm);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpPatch("{userId:guid}/deactivate")]
    public async Task<IResult> DeactivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateUserCommand(userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPatch("{userId:guid}/activate")]
    public async Task<IResult> ActivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new ActivateUserCommand(userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("{userId:guid}/roles")]
    public async Task<IResult> AssignRoleToUser(
        [FromRoute] Guid userId,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignRoleToUserCommand(userId, request.RoleName);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpDelete("{userId:guid}/roles/{roleName}")]
    public async Task<IResult> RemoveRoleFromUser(
        [FromRoute] Guid userId,
        [FromRoute] string roleName,
        CancellationToken cancellationToken)
    {
        var command = new RemoveRoleFromUserCommand(userId, roleName);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}