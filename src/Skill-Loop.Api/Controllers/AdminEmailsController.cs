using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Api.Extensions;
using Skill_Loop.Application.Features.Emails.Commands.TestEmail;
using Skill_Loop.Domain.Constants;
namespace Skill_Loop.Api.Controllers.v1.Admin;
/// <summary>
/// إدارة البريد الإلكتروني للمسؤولين (إرسال ومتابعة)
///
/// Security: role-gated here in addition to the per-command [Permission] check. A bare
/// [Authorize] previously let any authenticated user reach this controller, which combined
/// with an unrestricted recipient to expose the platform SMTP relay as an open spam relay.
/// </summary>
[Route("api/v1/Admin/Emails")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminEmailsController : BaseApiController
{
    [HttpPost("test")]
    [EnableRateLimiting(RateLimitingExtensions.EmailTestPolicy)]
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
