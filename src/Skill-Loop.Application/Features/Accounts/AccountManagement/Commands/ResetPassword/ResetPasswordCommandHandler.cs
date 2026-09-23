using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Web;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IOtpService otpService,                       // 👈 للتحقق من الكود
    IPasswordService passwordService,             // 👈 لتغيير كلمة المرور
    IUserManagementService userManagementService, // 👈 لجلب الاسم الحقيقي
    IJobScheduler jobScheduler,
    IClientContext clientContext) : ICommandHandler<ResetPasswordCommand>
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

        // 4. تجهيز نموذج البريد الإلكتروني بالبيانات الأساسية فقط للعرض
        var templateModel = new PasswordChangedTemplateModel
        {
            UserName = userName,
            UserEmail = request.Email
        };

        // 5. التقاط بيانات الاتصال من الـ HTTP Request الحالي قبل جدولتها
        var ipAddress = clientContext.IpAddress;
        var userAgent = clientContext.UserAgent;

        // 6. جدولة إرسال بريد "تم تغيير كلمة المرور" في الخلفية مع تمرير المعاملات
        jobScheduler.Enqueue<IIdentityNotificationService>(sender =>
            sender.SendPasswordChangedEmailAsync(
                request.Email,
                templateModel,
                ipAddress,
                userAgent));

        // 7. إرجاع استجابة النجاح
        return Result.Success("تم تغيير كلمة المرور بنجاح");
    }
}