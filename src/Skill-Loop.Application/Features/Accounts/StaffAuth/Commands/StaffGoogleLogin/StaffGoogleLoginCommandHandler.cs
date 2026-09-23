namespace Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffGoogleLogin;

using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Identity.Authentication;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Web;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Domain.Common.Results;

public sealed class StaffGoogleLoginCommandHandler(
    IStaffAuthService staffAuthService,
    IJobScheduler jobScheduler,
    IClientContext clientContext) : ICommandHandler<StaffGoogleLoginCommand, StaffAuthResponse>
{
    public async Task<Result<StaffAuthResponse>> Handle(
        StaffGoogleLoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. التحقق من Google Token وتسجيل الدخول
        var result = await staffAuthService.LoginWithGoogleAsync(
            request.IdToken,
            cancellationToken);

        if (result.IsFailure)
        {
            return result;
        }

        // 2. تجهيز نموذج قالب البريد الإلكتروني بالبيانات الأساسية فقط
        var templateModel = new LoginTemplateModel
        {
            UserName = result.Data!.FullName ?? string.Empty,
            UserEmail = result.Data.Email,
            LoginTime = result.Data.LoggedInAt
        };

        // 3. التقاط بيانات الاتصال من الـ HTTP Request النشط قبل الانتقال لـ Hangfire
        var ipAddress = clientContext.IpAddress;
        var userAgent = clientContext.UserAgent;

        // 4. جدولة إرسال بريد إشعار تسجيل الدخول في الخلفية مع تمرير المعاملات المنفصلة
        jobScheduler.Enqueue<IIdentityNotificationService>(sender =>
            sender.SendLoginEmailAsync(
                result.Data.Email,
                templateModel,
                ipAddress,
                userAgent));

        return result;
    }
}