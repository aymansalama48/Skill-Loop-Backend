namespace Skill_Loop.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.ResendEmailOtp;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;
using Skill_Loop.Api.Contracts.Auth;
/// <summary>
/// إدارة رموز التحقق (OTP) للبريد الإلكتروني والهاتف
/// </summary>
[Route("api/v1/auth/otp")]
[Tags("OTP Management")]
public class OtpController : BaseApiController
{
    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<IResult> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmailOtpCommand(request.Email, request.OtpCode);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("resend")]
    [AllowAnonymous]
    public async Task<IResult> ResendVerificationCode(
        [FromBody] ResendVerificationCodeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResendEmailOtpCommand(request.Email);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
