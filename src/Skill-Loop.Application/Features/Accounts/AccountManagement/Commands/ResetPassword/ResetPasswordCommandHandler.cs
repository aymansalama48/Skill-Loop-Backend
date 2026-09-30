using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Web;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;

/// <summary>
/// Security: resetting a password must evict every existing session. Otherwise the
/// reset-then-attack scenario fully succeeds — the attacker resets the password to lock the
/// legitimate user out, while their own refresh token (valid 7 days) keeps working. The
/// user is locked out AND the attacker remains authenticated.
/// </summary>
public sealed class ResetPasswordCommandHandler(
    IOtpService otpService,                       // 👈 للتحقق من الكود
    IPasswordService passwordService,             // 👈 لتغيير كلمة المرور
    IUserManagementService userManagementService, // 👈 لجلب الاسم الحقيقي
    IRefreshTokenService refreshTokenService,     // 👈 لإلغاء كل الجلسات القائمة
    IJobScheduler jobScheduler,
    IClientContext clientContext,
    ILogger<ResetPasswordCommandHandler> logger) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // 1. التحقق من صحة كود الـ OTP أولاً
        var otpValidationResult = await otpService.ValidateOtpAsync(
            request.Email,
            request.OtpCode,
            OtpPurpose.PasswordReset,
            cancellationToken);

        if (otpValidationResult.IsFailure)
        {
            return otpValidationResult;
        }

        // 2. إعادة تعيين كلمة المرور
        var resetResult = await passwordService.ResetPasswordAsync(
            request.Email,
            request.NewPassword,
            cancellationToken);

        if (resetResult.IsFailure)
        {
            return resetResult;
        }

        // 3. جلب بيانات المستخدم للاسم الحقيقي
        var userResult = await userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);
        var userName = userResult.IsSuccess ? userResult.Data!.FirstName : string.Empty;

        // 4. إلغاء كل الجلسات القائمة — ده هو الغرض الفعلي من إعادة التعيين
        if (userResult.IsSuccess)
        {
            var userId = userResult.Data!.Id;

            var revokeResult = await refreshTokenService
                .RevokeAllUserTokensAsync(userId, cancellationToken);

            if (revokeResult.IsFailure)
            {
                // Do not fail the reset — the password already changed. Surface it loudly.
                logger.LogError(
                    "Password reset for {UserId} succeeded but session revocation failed. " +
                    "Existing sessions may remain valid.",
                    userId);
            }
            else
            {
                logger.LogInformation(
                    "All sessions revoked after password reset for user {UserId}.", userId);
            }
        }

        // 5. تجهيز نموذج البريد الإلكتروني بالبيانات الأساسية فقط للعرض
        var templateModel = new PasswordChangedTemplateModel
        {
            UserName = userName,
            UserEmail = request.Email
        };

        // 6. التقاط بيانات الاتصال من الـ HTTP Request النشط قبل جدولتها
        var ipAddress = clientContext.IpAddress;
        var userAgent = clientContext.UserAgent;

        // 7. جدولة إرسال بريد "تم تغيير كلمة المرور" في الخلفية مع تمرير المعاملات
        jobScheduler.Enqueue<IIdentityNotificationService>(sender =>
            sender.SendPasswordChangedEmailAsync(
                request.Email,
                templateModel,
                ipAddress,
                userAgent));

        // 8. إرجاع استجابة النجاح
        return Result.Success("تم تغيير كلمة المرور بنجاح. تم تسجيل خروجك من جميع الأجهزة.");
    }
}
