using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;

namespace Skill_Loop.Application.Common.Abstractions.Notifications;

/// <summary>
/// خدمة إشعارات الهوية (إيميلات الترحيب، تأكيد الحساب، إعادة التعيين، إلخ).
///
/// ⚠️ ملاحظة معمارية مهمة:
/// الدوال اللي بتعرض بيانات العميل (الجهاز، IP، الموقع) تستقبل ipAddress و userAgent
/// كـ Parameters اختيارية. السبب: الخدمة دي بتشتغل في سياق Hangfire Worker Thread
/// حيث HttpContext دايماً null، فما ينفعش نحقن IClientContext أو IHttpContextAccessor.
/// المستدعي (في الـ HTTP Request) هو اللي يقرأهم ويمررهم هنا.
/// </summary>
public interface IIdentityNotificationService
{
    Task SendLoginEmailAsync(
        string email,
        LoginTemplateModel model,
        string? ipAddress = null,
        string? userAgent = null);

    Task SendEmailConfirmationAsync(
        string email,
        EmailConfirmationTemplateModel model,
        string? ipAddress = null,
        string? userAgent = null);

    Task SendResetPasswordEmailAsync(
        string email,
        ResetPasswordTemplateModel model,
        string? ipAddress = null,
        string? userAgent = null);

    Task SendWelcomeEmailAsync(
        string email,
        WelcomeTemplateModel model);

    Task SendPasswordChangedEmailAsync(
        string email,
        PasswordChangedTemplateModel model,
        string? ipAddress = null,
        string? userAgent = null);

    Task SendAccountLockedEmailAsync(
        string email,
        AccountLockedTemplateModel model);

    Task SendAccountUnlockedEmailAsync(
        string email,
        AccountUnlockedTemplateModel model);

    Task SendRoleAssignedEmailAsync(
        string email,
        RoleAssignedTemplateModel model);

    Task SendRoleRemovedEmailAsync(
        string email,
        RoleRemovedTemplateModel model);

    Task SendStaffInvitationEmailAsync(
        string email,
        StaffInvitationTemplateModel model);
}