using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Web;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IPasswordService passwordService,
    ICurrentUser currentUser,
    IJobScheduler jobScheduler,
    IClientContext clientContext) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        // 1. جلب معرف البريد والمعرف الخاص بالمستخدم الحالي
        var userId = currentUser.UserId;
        var userEmail = currentUser.Email;

        if (userId is null || string.IsNullOrEmpty(userEmail))
        {
            return Result.Failure(UserErrors.NotFound);
        }

        // 2. تغيير كلمة المرور عبر الخدمة
        var result = await passwordService.ChangePasswordAsync(
            userId.Value,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);

        // 3. التحقق من نجاح عملية التغيير
        if (result.IsFailure)
        {
            return result;
        }

        // 4. تجهيز نموذج البريد الإلكتروني مع البيانات الأساسية فقط
        var templateModel = new PasswordChangedTemplateModel
        {
            UserName = currentUser.FullName ?? string.Empty,
            UserEmail = userEmail
        };

        // 5. التقاط بيانات الاتصال من الـ HTTP Request الحالي قبل إرسال المهمة للـ Background Job
        var ipAddress = clientContext.IpAddress;
        var userAgent = clientContext.UserAgent;

        // 6. جدولة إرسال بريد الإشعار في الخلفية عبر Hangfire مع تمرير ipAddress و userAgent
        jobScheduler.Enqueue<IIdentityNotificationService>(sender =>
            sender.SendPasswordChangedEmailAsync(
                userEmail,
                templateModel,
                ipAddress,
                userAgent));

        // 7. إرجاع استجابة النجاح فوراً
        return Result.Success("تم تغيير كلمة المرور بنجاح");
    }
}