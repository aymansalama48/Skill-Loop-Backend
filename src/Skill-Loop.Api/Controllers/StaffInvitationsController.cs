namespace Skill_Loop.Api.Controllers;


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.StaffInvitations;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitation;
using Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.AcceptInvitationWithGoogle;
using Skill_Loop.Application.Features.Accounts.StaffInvitations.Commands.SendInvitation;
using Skill_Loop.Application.Features.Accounts.StaffInvitations.Queries.ValidateInvitation;

[Route("api/v1/[controller]")]
public class StaffInvitationsController : BaseApiController
{
    /// <summary>
    /// إرسال دعوة لموظف جديد (خاص بالأدمن فقط)
    /// </summary>
    [HttpPost("send")]
    //[Authorize(Roles = "Admin")]
    public async Task<IResult> SendInvitation(
        [FromBody] SendStaffInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SendStaffInvitationCommand(
            request.Email,
            request.Role);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// التحقق من صلاحية التوكن (يُستدعى من الـ Frontend عند فتح صفحة قبول الدعوة)
    /// </summary>
    [HttpGet("validate/{token}")]
    [AllowAnonymous]
    public async Task<IResult> ValidateInvitation(
        string token,
        CancellationToken cancellationToken)
    {
        var query = new ValidateInvitationQuery(token);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// قبول الدعوة وإنشاء الحساب (بالطريقة التقليدية: إيميل وباسورد)
    /// </summary>
    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<IResult> AcceptInvitation(
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationCommand(
            request.InvitationToken,
            request.FullName,
            request.Password,
            request.PhoneNumber);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// قبول الدعوة وإنشاء الحساب (عبر جوجل)
    /// </summary>
    [HttpPost("accept-google")]
    [AllowAnonymous]
    public async Task<IResult> AcceptInvitationWithGoogle(
        [FromBody] AcceptInvitationWithGoogleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationWithGoogleCommand(
            request.InvitationToken,
            request.GoogleIdToken);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}