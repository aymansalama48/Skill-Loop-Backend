namespace Skill_Loop.Api.Controllers;

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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


[Route("api/[controller]")]
public class AccountsController : BaseApiController
{
    /// <summary>
    /// تسجيل دخول الموظفين (Admin / Doctor / Receptionist) عبر البريد وكلمة السر
    /// </summary>
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

    /// <summary>
    /// تسجيل دخول الموظفين عبر Google OAuth
    /// </summary>
    [HttpPost("google-login")]
    [AllowAnonymous]
    public async Task<IResult> GoogleLogin(
        [FromBody] StaffGoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StaffGoogleLoginCommand(request.IdToken);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تجديد الـ Access Token باستخدام الـ Refresh Token
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

    /// <summary>
    /// طلب رابط إعادة تعيين كلمة المرور (نسيت كلمة المرور)
    /// </summary>
    [HttpPost("forgot-password")]
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
    /// إعادة تعيين كلمة المرور باستخدام الـ Token
    /// </summary>
    [HttpPost("reset-password")]
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
    /// تغيير كلمة المرور للمستخدم المسجل حالياً
    /// </summary>
    [HttpPost("change-password")]
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

    [HttpGet("me/profile")]
    [Authorize] // متاح لأي مستخدم مسجل الدخول
    public async Task<IResult> GetMyAccountProfile(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyAccountProfileQuery(), cancellationToken);
        return HandleResult(result);
    }
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetAllUsers(
        [FromQuery] GetUsersRequest request,
        CancellationToken cancellationToken)
    {
        // Mapping من الـ Contract للـ Query
        var query = new GetAllUsersQuery(
            request.PageNumber,
            request.PageSize,
            request.Role,
            request.SearchTerm);

        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("me/profile")]
    [Authorize]
    public async Task<IResult> UpdateMyAccountProfile(
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

    [HttpPut("me/profile/picture")]
    [Authorize]
    public async Task<IResult> UpdateMyProfilePicture(
        [FromBody] UpdateMyProfilePictureRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMyProfilePictureCommand(request.AvatarUrl);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPut("{userId:guid}/deactivate")]
    [Authorize(Roles = "Admin")] // 👈 حماية للآدمن فقط
    public async Task<IResult> DeactivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateUserCommand(userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{userId:guid}/activate")]
    [Authorize(Roles = "Admin")] // 👈 حماية للآدمن فقط
    public async Task<IResult> ActivateUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new ActivateUserCommand(userId);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    /// <summary>
    /// إضافة دور لمستخدم
    /// </summary>
    [HttpPost("{userId:guid}/roles")]
    [Authorize(Roles = "Admin")] // تأكد من وضع الصلاحية المناسبة
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
    /// سحب دور من مستخدم
    /// </summary>
    [HttpDelete("{userId:guid}/roles/{roleName}")]
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
}