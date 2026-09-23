using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.Authentication;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Web;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffLogin;

public sealed class StaffLoginCommandHandler(
    IStaffAuthService staffAuthService,
    IJobScheduler jobScheduler,
    IClientContext clientContext)
    : ICommandHandler<StaffLoginCommand, StaffAuthResponse>
{
    public async Task<Result<StaffAuthResponse>> Handle(
        StaffLoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. تنفيذ عملية تسجيل الدخول
        var result = await staffAuthService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        // 2. إذا فشل تسجيل الدخول نرجع النتيجة فوراً
        if (result.IsFailure)
        {
            return result;
        }

        // 3. تجهيز بيانات إشعار تسجيل الدخول للعرض فقط
        var templateModel = new LoginTemplateModel
        {
            UserName = result.Data!.FullName ?? string.Empty,
            UserEmail = result.Data.Email,
            LoginTime = result.Data.LoggedInAt
        };

        // 4. التقاط بيانات الاتصال من الـ HTTP Request الحالي
        var ipAddress = clientContext.IpAddress;
        var userAgent = clientContext.UserAgent;

        // 5. جدولة إرسال الإيميل كـ Background Job مع تمرير المعاملات المنفصلة
        jobScheduler.Enqueue<IIdentityNotificationService>(
            sender => sender.SendLoginEmailAsync(
                request.Email,
                templateModel,
                ipAddress,
                userAgent));

        // 6. إرجاع النتيجة فوراً دون انتظار إرسال الإيميل
        return result;
    }
}