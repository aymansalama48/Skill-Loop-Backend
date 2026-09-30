namespace Skill_Loop.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Api.Extensions;
using Skill_Loop.Application.Features.Accounts.Authentication.Commands.Logout;
using Skill_Loop.Application.Features.Accounts.Authentication.Commands.RefreshToken;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffGoogleLogin;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffLogin;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.LoginUser;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.RegisterUser;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.ResendEmailOtp;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.UserGoogleLogin;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.UserLogin;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ForgotPassword;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;
using Skill_Loop.Api.Contracts.Auth;
/// <summary>
/// إدارة المصادقة وتسجيل الدخول للمستخدمين
/// </summary>
[Route("api/v1/[controller]")]
public class AuthController : BaseApiController
{
    // =========================================================================
    // 1. Staff Authentication (لوحة التحكم)
    // =========================================================================
    [HttpPost("staff/login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    public async Task<IResult> StaffLogin(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StaffLoginCommand(request.Email, request.Password);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("staff/login/google")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    public async Task<IResult> StaffGoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StaffGoogleLoginCommand(request.IdToken);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    // =========================================================================
    // 2. User Authentication (الموبايل / المستخدم العادي)
    // =========================================================================
    [HttpPost("user/login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    public async Task<IResult> UserLogin(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UserLoginCommand(request.Email, request.Password);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("user/login/google")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    public async Task<IResult> UserGoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UserGoogleLoginCommand(request.IdToken);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("user/register")]
    [AllowAnonymous]
    // Registration creates an account and sends an email per call, so it is throttled
    // like the OTP resend flow to prevent mailbox/SMTP flooding and bulk account creation.
    [EnableRateLimiting(RateLimitingExtensions.OtpResendPolicy)]
    public async Task<IResult> RegisterUser(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    // =========================================================================
    // 3. Token Management
    // =========================================================================
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.RefreshPolicy)]
    public async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPost("logout")]
    [Authorize]
    public async Task<IResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    // =========================================================================
}
