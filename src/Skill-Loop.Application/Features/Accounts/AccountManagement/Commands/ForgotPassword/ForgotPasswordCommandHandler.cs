using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Web;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    IUserManagementService userManagementService, // 👈 لجلب بيانات واسم المستخدم
    IOtpService otpService,                       // 👈 لتوليد كود الـ 4 أرقام
    IJobScheduler jobScheduler,
    IClientContext clientContext) : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // 1. جلب بيانات المستخدم بالكامل
        var userResult = await userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);

        // 2. حماية أمنية (Anti-Enumeration): 
        // إذا كان الحساب غير موجود، نرجع "نجاح" وهمي حتى لا يعرف المخترق أن الإيميل غير مسجل
        if (userResult.IsFailure)
        {
            return Result.Success();
        }

        // 3. إنشاء كود OTP من 4 أرقام لإعادة تعيين كلمة المرور
        var otpResult = await otpService.GenerateOtpAsync(
            request.Email,
            OtpPurpose.PasswordReset,
            cancellationToken);

        if (otpResult.IsFailure)
        {
            return Result.Success(); // نفس الحماية الأمنية
        }

        // 4. تجهيز نموذج قالب البريد الإلكتروني بالاسم الحقيقي وبيانات الأمان
        var templateModel = new ResetPasswordTemplateModel
        {
            UserName = userResult.Data!.FirstName, // 👈 الاسم الحقيقي
            UserEmail = request.Email,
            OtpCode = otpResult.Data.Code,         // 👈 الكود من 4 أرقام
            IpAddress = clientContext.IpAddress ?? string.Empty,
            UserAgent = clientContext.UserAgent ?? string.Empty
        };

        // 5. جدولة إرسال البريد كـ Background Job
        jobScheduler.Enqueue<IIdentityNotificationService>(
            sender => sender.SendResetPasswordEmailAsync(
                request.Email,
                templateModel));

        // 6. إرجاع استجابة النجاح
        return Result.Success();
    }
}