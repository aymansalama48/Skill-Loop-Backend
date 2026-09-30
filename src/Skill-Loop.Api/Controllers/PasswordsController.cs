namespace Skill_Loop.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Api.Extensions;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ForgotPassword;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;
using Skill_Loop.Api.Contracts.Auth;
/// <summary>
/// إدارة إعادة تعيين واستعادة كلمات المرور
/// </summary>
[Route("api/v1/auth/password")]
[Tags("Passwords")]
public class PasswordsController : BaseApiController
{
    [HttpPost("forgot")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.OtpResendPolicy)]
    public async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("reset")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.OtpVerifyPolicy)]
    public async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(
            request.Email,
            request.OtpCode,
            request.NewPassword,
            request.ConfirmPassword);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
