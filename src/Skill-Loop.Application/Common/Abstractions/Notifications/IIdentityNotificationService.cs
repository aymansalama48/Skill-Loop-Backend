using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.SessionsTemplates;

namespace Skill_Loop.Application.Common.Abstractions.Notifications;

public interface IIdentityNotificationService
{
    Task SendLoginEmailAsync(string email, LoginTemplateModel model);
    Task SendEmailConfirmationAsync(string email, EmailConfirmationTemplateModel model);
    Task SendResetPasswordEmailAsync(string email, ResetPasswordTemplateModel model);
    Task SendWelcomeEmailAsync(string email, WelcomeTemplateModel model);
    Task SendPasswordChangedEmailAsync(string email, PasswordChangedTemplateModel model);
    Task SendAccountLockedEmailAsync(string email, AccountLockedTemplateModel model);
    Task SendAccountUnlockedEmailAsync(string email, AccountUnlockedTemplateModel model);
    Task SendRoleAssignedEmailAsync(string email, RoleAssignedTemplateModel model);
    Task SendRoleRemovedEmailAsync(string email, RoleRemovedTemplateModel model);
    Task SendStaffInvitationEmailAsync(string email, StaffInvitationTemplateModel model);

    Task SendMaterialUploadedEmailAsync(string email, SessionMaterialUploadedTemplateModel model);

    Task SendMaterialUploadedConfirmationAsync(string email, SessionMaterialUploadedTemplateModel model);

    Task SendStorageQuotaWarningAsync(long usedBytes, long totalBytes, double threshold);
}
