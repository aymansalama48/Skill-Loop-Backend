using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Web;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IPasswordService passwordService,
    IJobScheduler jobScheduler,
    IClientContext clientContext) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // 1. إعادة تعيين كلمة المرور عبر خدمة الهوية
        var result = await passwordService.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword,
            cancellationToken);

        // 2. التحقق من نجاح العملية قبل متابعة الإرسال
        if (result.IsFailure)
        {
            return result;
        }

        // 3. تجهيز نموذج البريد الإلكتروني مع بيانات الاتصال واسم المستخدم (إن وجد)
        var templateModel = new PasswordChangedTemplateModel
        {
            IpAddress = clientContext.IpAddress ?? string.Empty,
            UserAgent = clientContext.UserAgent ?? string.Empty
        };

        // 4. جدولة إرسال بريد التأكيد في الخلفية
        jobScheduler.Enqueue<IIdentityNotificationService>(sender =>
            sender.SendPasswordChangedEmailAsync(
                request.Email,
                templateModel));

        // 5. إرجاع استجابة النجاح فوراً
        return Result.Success("تم تغيير كلمة المرور بنجاح");
    }
}