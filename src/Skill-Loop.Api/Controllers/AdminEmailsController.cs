using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Emails.Commands.TestEmail;
using Skill_Loop.Domain.Constants;
namespace Skill_Loop.Api.Controllers.v1.Admin;
/// <summary>
/// إدارة البريد الإلكتروني للمسؤولين (إرسال ومتابعة)
/// </summary>
[Route("api/v1/Admin/Emails")]
[Authorize(Roles = Roles.SuperAdmin)]
public class AdminEmailsController : BaseApiController
{
    [HttpPost("test")]
    public async Task<IResult> SendTestEmail([FromBody] TestEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("{id}/resend")]
    public async Task<IResult> ResendEmail([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new Skill_Loop.Application.Features.Emails.Commands.ResendEmail.ResendEmailCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
