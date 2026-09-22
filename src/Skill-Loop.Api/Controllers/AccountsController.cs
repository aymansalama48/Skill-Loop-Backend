namespace Skill_Loop.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Accounts;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ActivateUser;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.AssignRoleToUser;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ChangePassword;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.DeactivateUser;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ForgotPassword;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.RemoveRoleFromUser;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfile;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfilePicture;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetAllUsers;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetMyProfile;
using Skill_Loop.Application.Features.Accounts.Authentication.Commands.Logout;
using Skill_Loop.Application.Features.Accounts.Authentication.Commands.RefreshToken;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffGoogleLogin;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffLogin;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.RegisterUser;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.ResendEmailOtp;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;

[Route("api/[controller]")]
public class AccountsController : BaseApiController
{
    // =========================================================================
    // 1. Authentication (المصادقة)
    // =========================================================================

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IResult> Login(
        [FromBody] StaffLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StaffLoginCommand(request.Email, request.Password);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("login/google")]
    [AllowAnonymous]
    public async Task<IResult> GoogleLogin(
        [FromBody] StaffGoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StaffGoogleLoginCommand(request.IdToken);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
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
    // 2. Password Management (إدارة كلمة المرور)
    // =========================================================================

    [HttpPost("password/forgot")]
    [AllowAnonymous]
    public async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("password/reset")]
    [AllowAnonymous]
    public async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(
            request.Email,
            request.Token,
            request.NewPassword,
            request.ConfirmPassword);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("me/change-password")] 
    [Authorize]
    public async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // =========================================================================
    // 3. Current User Profile Management (إدارة الملف الشخصي للمستخدم الحالي)
    // =========================================================================

    [HttpGet("me/get-profile")] 
    [Authorize]
    public async Task<IResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyAccountProfileQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("me/update-profile")]
    [Authorize]
    public async Task<IResult> UpdateMyProfile(
        [FromBody] UpdateMyAccountProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMyAccountProfileCommand(
            request.FirstName,
            request.LastName,
            request.PhoneNumber);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPatch("me/update-profile-picture")] 
    [Authorize]
    public async Task<IResult> UpdateMyProfilePicture(
        [FromBody] UpdateMyProfilePictureRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMyProfilePictureCommand(request.AvatarUrl);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // =========================================================================
    // 4. User Administration (إدارة المستخدمين - للآدمن)
    // =========================================================================

    [HttpGet("get-all-users")] 
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeactivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateUserCommand(userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPatch("{userId:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> ActivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new ActivateUserCommand(userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // =========================================================================
    // 5. Role Management (إدارة الصلاحيات والأدوار)
    // =========================================================================

    [HttpPost("{userId:guid}/assign-role")] 
    [Authorize(Roles = "Admin")]
    public async Task<IResult> AssignRoleToUser(
        [FromRoute] Guid userId,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignRoleToUserCommand(userId, request.RoleName);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{userId:guid}/remove-role/{roleName}")] // استخدام اسم صريح
    [Authorize(Roles = "Admin")]
    public async Task<IResult> RemoveRoleFromUser(
        [FromRoute] Guid userId,
        [FromRoute] string roleName,
        CancellationToken cancellationToken)
    {
        var command = new RemoveRoleFromUserCommand(userId, roleName);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // =========================================================================
    // 6. Registration & Verification (التسجيل والتأكيد)
    // =========================================================================

    /// <summary>
    /// تسجيل حساب جديد (وإرسال OTP للإيميل)
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IResult> Register(
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

    /// <summary>
    /// تأكيد البريد الإلكتروني باستخدام كود OTP
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IResult> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmailOtpCommand(request.Email, request.OtpCode);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إعادة إرسال كود الـ OTP لتأكيد الإيميل
    /// </summary>
    [HttpPost("resend-verification-code")]
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