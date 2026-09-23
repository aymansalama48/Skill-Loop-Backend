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
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.LoginUser;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.RegisterUser;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.ResendEmailOtp;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.UserGoogleLogin;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.UserLogin;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;

[Route("api/[controller]")]
public class AccountsController : BaseApiController
{
    // =========================================================================
    // 1. Staff Authentication (مصادقة الـ Staff — لوحة التحكم)
    // =========================================================================

    /// <summary>
    /// تسجيل دخول الـ Staff (Admin, SuperAdmin, FinanceManager, Support)
    /// </summary>
    [HttpPost("staff/login")]
    [AllowAnonymous]
    public async Task<IResult> StaffLogin(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StaffLoginCommand(request.Email, request.Password);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تسجيل دخول الـ Staff عبر Google
    /// </summary>
    [HttpPost("staff/login/google")]
    [AllowAnonymous]
    public async Task<IResult> StaffGoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StaffGoogleLoginCommand(request.IdToken);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // =========================================================================
    // 2. User Authentication (مصادقة مستخدم الموبايل العادي)
    // =========================================================================

    /// <summary>
    /// تسجيل دخول المستخدم العادي (Learner / Instructor) بالـ Email + Password
    /// </summary>
    [HttpPost("user/login")]
    [AllowAnonymous]
    public async Task<IResult> UserLogin(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UserLoginCommand(request.Email, request.Password);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تسجيل دخول المستخدم العادي عبر Google
    /// </summary>
    [HttpPost("user/login/google")]
    [AllowAnonymous]
    public async Task<IResult> UserGoogleLogin(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UserGoogleLoginCommand(request.IdToken);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تسجيل حساب مستخدم جديد (Learner) + إرسال OTP للإيميل
    /// </summary>
    [HttpPost("user/register")]
    [AllowAnonymous]
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

    /// <summary>
    /// تأكيد البريد الإلكتروني باستخدام كود OTP
    /// </summary>
    [HttpPost("user/verify-email")]
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
    [HttpPost("user/resend-verification-code")]
    [AllowAnonymous]
    public async Task<IResult> ResendVerificationCode(
        [FromBody] ResendVerificationCodeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResendEmailOtpCommand(request.Email);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // =========================================================================
    // 3. Token Management (مشترك بين Staff و User)
    // =========================================================================

    /// <summary>
    /// تجديد الـ Access Token باستخدام Refresh Token
    /// </summary>
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

    /// <summary>
    /// تسجيل الخروج وإبطال الـ Refresh Token
    /// </summary>
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
    // 4. Password Management (إدارة كلمة المرور)
    // =========================================================================

    /// <summary>
    /// طلب استعادة كلمة المرور (إرسال OTP للإيميل)
    /// </summary>
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

    /// <summary>
    /// إعادة تعيين كلمة المرور باستخدام كود الـ OTP
    /// </summary>
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

    /// <summary>
    /// تغيير كلمة المرور للمستخدم الحالي
    /// </summary>
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
    // 5. Current User Profile (الملف الشخصي للمستخدم الحالي)
    // =========================================================================

    /// <summary>
    /// جلب بيانات المستخدم الحالي
    /// </summary>
    [HttpGet("me/get-profile")]
    [Authorize]
    public async Task<IResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyAccountProfileQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تحديث بيانات المستخدم الحالي
    /// </summary>
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

    /// <summary>
    /// تحديث صورة المستخدم الحالي
    /// </summary>
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
    // 6. User Administration (إدارة المستخدمين — Staff فقط)
    // =========================================================================

    /// <summary>
    /// جلب كل المستخدمين مع Pagination والفلترة
    /// </summary>
    [HttpGet("admin/users")]
 //   [Authorize(Roles = "Admin,SuperAdmin")]
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

    /// <summary>
    /// إيقاف حساب مستخدم
    /// </summary>
    [HttpPatch("admin/users/{userId:guid}/deactivate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IResult> DeactivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateUserCommand(userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تفعيل حساب مستخدم
    /// </summary>
    [HttpPatch("admin/users/{userId:guid}/activate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IResult> ActivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new ActivateUserCommand(userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // =========================================================================
    // 7. Role Management (إدارة الأدوار — Staff فقط)
    // =========================================================================

    /// <summary>
    /// تعيين دور لمستخدم
    /// </summary>
    [HttpPost("admin/users/{userId:guid}/assign-role")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IResult> AssignRoleToUser(
        [FromRoute] Guid userId,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignRoleToUserCommand(userId, request.RoleName);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إزالة دور من مستخدم
    /// </summary>
    [HttpDelete("admin/users/{userId:guid}/remove-role/{roleName}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
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