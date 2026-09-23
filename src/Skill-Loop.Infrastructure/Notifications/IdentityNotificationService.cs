using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.External.Client;
using Skill_Loop.Application.Common.Abstractions.External.Client.Models;
using Skill_Loop.Application.Common.Abstractions.External.Email;
using Skill_Loop.Application.Common.Abstractions.External.Email.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.SessionsTemplates;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Infrastructure.External.Email;
using Skill_Loop.Infrastructure.Options;
using System.Reflection;

namespace Skill_Loop.Infrastructure.Notifications;

public sealed class IdentityNotificationService : IIdentityNotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly EmailTemplateEngine _templateEngine;
    private readonly BaseUrlOptions _baseUrlOptions;
    private readonly IDateTime _dateTimeProvider;
    private readonly IUserAgentParser _userAgentParser;
    private readonly IGeoLocationService _geoLocationService;

    // تم تغيير اسم التطبيق الافتراضي
    private const string AppName = "Skill Loop";

    public IdentityNotificationService(
        IEmailSender emailSender,
        EmailTemplateEngine templateEngine,
        IOptions<BaseUrlOptions> baseUrlOptions,
        IDateTime dateTimeProvider,
        IUserAgentParser userAgentParser,
        IGeoLocationService geoLocationService)
    {
        _emailSender = emailSender;
        _templateEngine = templateEngine;
        _baseUrlOptions = baseUrlOptions.Value;
        _dateTimeProvider = dateTimeProvider;
        _userAgentParser = userAgentParser;
        _geoLocationService = geoLocationService;
    }

    public async Task SendLoginEmailAsync(string email, LoginTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? "المستخدم العزيز" : model.UserName;
        if (model.LoginTime == default) model.LoginTime = _dateTimeProvider.Now; // استخدام UtcNow

        await PopulateClientInfoAsync(model, model.UserAgent, model.IpAddress);
        await SendTemplateEmailAsync(email, "إشعار تسجيل دخول جديد", EmailTemplateNames.Login, model);
    }

    public async Task SendEmailConfirmationAsync(string email, EmailConfirmationTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? "المستخدم العزيز" : model.UserName;
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? email : model.Email;
        model.ConfirmationLink = string.IsNullOrWhiteSpace(model.ConfirmationLink)
            ? $"{_baseUrlOptions.Frontend}/confirm-email"
            : model.ConfirmationLink;

        await PopulateClientInfoAsync(model, model.UserAgent, model.IpAddress);
        await SendTemplateEmailAsync(email, "تأكيد بريدك الإلكتروني", EmailTemplateNames.EmailConfirmation, model);
    }

    public async Task SendResetPasswordEmailAsync(string email, ResetPasswordTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? "المستخدم العزيز" : model.UserName;
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? email : model.Email;
        model.ResetLink = string.IsNullOrWhiteSpace(model.ResetLink)
            ? $"{_baseUrlOptions.Frontend}/reset-password"
            : model.ResetLink;

        await PopulateClientInfoAsync(model, model.UserAgent, model.IpAddress);
        await SendTemplateEmailAsync(email, "إعادة تعيين كلمة المرور", EmailTemplateNames.ResetPassword, model);
    }

    public async Task SendWelcomeEmailAsync(string email, WelcomeTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? "المستخدم العزيز" : model.UserName;
        model.LoginUrl = string.IsNullOrWhiteSpace(model.LoginUrl)
            ? $"{_baseUrlOptions.Frontend}/login"
            : model.LoginUrl;

        await SendTemplateEmailAsync(email, $"مرحباً بك في {AppName}", EmailTemplateNames.Welcome, model);
    }

    public async Task SendPasswordChangedEmailAsync(string email, PasswordChangedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? "المستخدم العزيز" : model.UserName;
        if (model.ChangedAt == default) model.ChangedAt = _dateTimeProvider.Now; // استخدام UtcNow

        await PopulateClientInfoAsync(model, model.UserAgent, model.IpAddress);
        await SendTemplateEmailAsync(email, "تم تغيير كلمة المرور", EmailTemplateNames.PasswordChanged, model);
    }

    public async Task SendAccountLockedEmailAsync(string email, AccountLockedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? "المستخدم العزيز" : model.UserName;
        model.UnlockUrl = string.IsNullOrWhiteSpace(model.UnlockUrl)
            ? $"{_baseUrlOptions.Frontend}/support"
            : model.UnlockUrl;

        await SendTemplateEmailAsync(email, "تم قفل حسابك", EmailTemplateNames.AccountLocked, model);
    }

    public async Task SendAccountUnlockedEmailAsync(string email, AccountUnlockedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? "المستخدم العزيز" : model.UserName;
        model.LoginUrl = string.IsNullOrWhiteSpace(model.LoginUrl)
            ? $"{_baseUrlOptions.Frontend}/login"
            : model.LoginUrl;

        await SendTemplateEmailAsync(email, "تم فتح قفل حسابك", EmailTemplateNames.AccountUnlocked, model);
    }

    public async Task SendRoleAssignedEmailAsync(string email, RoleAssignedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? "المستخدم العزيز" : model.UserName;
        model.RoleName = string.IsNullOrWhiteSpace(model.RoleName) ? "مستخدم" : model.RoleName;
        model.DashboardUrl = string.IsNullOrWhiteSpace(model.DashboardUrl)
            ? $"{_baseUrlOptions.Frontend}/dashboard"
            : model.DashboardUrl;

        await SendTemplateEmailAsync(email, "تم تعيين دور جديد لك", EmailTemplateNames.RoleAssigned, model);
    }

    public async Task SendRoleRemovedEmailAsync(string email, RoleRemovedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? "المستخدم العزيز" : model.UserName;
        model.RoleName = string.IsNullOrWhiteSpace(model.RoleName) ? "مستخدم" : model.RoleName;

        await SendTemplateEmailAsync(email, "تم إزالة دورك", EmailTemplateNames.RoleRemoved, model);
    }

    public async Task SendStaffInvitationEmailAsync(string email, StaffInvitationTemplateModel model)
    {
        model.RoleName = string.IsNullOrWhiteSpace(model.RoleName) ? "موظف" : model.RoleName;
        model.AdminName = string.IsNullOrWhiteSpace(model.AdminName) ? "إدارة النظام" : model.AdminName;
        model.InvitedEmail = string.IsNullOrWhiteSpace(model.InvitedEmail) ? email : model.InvitedEmail;

        if (string.IsNullOrWhiteSpace(model.InvitationLink) && !string.IsNullOrWhiteSpace(model.Token))
        {
            model.InvitationLink = $"{_baseUrlOptions.Frontend}/accept-invitation?token={model.Token}";
        }

        await SendTemplateEmailAsync(email, "دعوة للانضمام إلى فريق العمل", "StaffInvitation", model);
    }

    public async Task SendMaterialUploadedEmailAsync(string email, SessionMaterialUploadedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? email : model.UserName;
        await SendTemplateEmailAsync(email, $"تم رفع مادة جديدة: {model.MaterialName}", "SessionMaterialUploaded", model);
    }

    public async Task SendMaterialUploadedConfirmationAsync(string email, SessionMaterialUploadedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName) ? email : model.UserName;
        await SendTemplateEmailAsync(email, $"تأكيد رفع مادة: {model.MaterialName}", "SessionMaterialUploaded", model);
    }

    public async Task SendStorageQuotaWarningAsync(long usedBytes, long totalBytes, double threshold)
    {
        var model = new StorageQuotaWarningTemplateModel
        {
            UsedBytes = usedBytes,
            TotalBytes = totalBytes,
            Threshold = threshold,
            UsedFormatted = FormatBytes(usedBytes),
            TotalFormatted = FormatBytes(totalBytes),
        };

        await SendTemplateEmailAsync("admin@skill-loop.com", "تنبيه: اقتراب حدود تخزين Google Drive", "StorageQuotaWarning", model);
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1024 * 1024 * 1024) return $"{(bytes / (1024.0 * 1024 * 1024)):F2} GB";
        if (bytes >= 1024 * 1024) return $"{(bytes / (1024.0 * 1024)):F2} MB";
        if (bytes >= 1024) return $"{(bytes / 1024.0):F2} KB";
        return $"{bytes} B";
    }

    // ============================================================
    // الدوال المساعدة
    // ============================================================

    private async Task PopulateClientInfoAsync<TModel>(TModel model, string? userAgent, string? ipAddress)
        where TModel : class
    {
        SetPropertyValue(model, "IpAddress", string.IsNullOrWhiteSpace(ipAddress) ? "غير معروف" : ipAddress);
        SetPropertyValue(model, "Device", "جهاز غير معروف");
        SetPropertyValue(model, "Location", "موقع غير معروف");

        try
        {
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                var deviceInfo = _userAgentParser.Parse(userAgent);
                SetPropertyValue(model, "Device", BuildDeviceName(deviceInfo));
            }
        }
        catch { /* تجاهل الأخطاء */ }

        try
        {
            var currentIp = string.IsNullOrWhiteSpace(ipAddress) ? "غير معروف" : ipAddress;
            if (currentIp != "غير معروف")
            {
                var location = await _geoLocationService.GetLocationAsync(currentIp, CancellationToken.None);
                if (!string.IsNullOrWhiteSpace(location))
                {
                    SetPropertyValue(model, "Location", location);
                }
            }
        }
        catch { /* تجاهل الأخطاء */ }
    }

    private static void SetPropertyValue<TModel>(TModel model, string propertyName, string value)
        where TModel : class
    {
        if (model == null) return;

        var prop = typeof(TModel).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(model, value);
        }
    }

    private static string BuildDeviceName(ClientDeviceInfo device)
    {
        var browser = device.Browser?.Trim();
        var operatingSystem = device.OperatingSystem?.Trim();

        if (string.IsNullOrWhiteSpace(browser) && string.IsNullOrWhiteSpace(operatingSystem))
            return "جهاز غير معروف";

        if (string.IsNullOrWhiteSpace(browser)) return operatingSystem!;
        if (string.IsNullOrWhiteSpace(operatingSystem)) return browser!;

        return $"{browser} on {operatingSystem}";
    }

    private async Task SendTemplateEmailAsync<TModel>(string to, string subject, string templateName, TModel model)
        where TModel : BaseEmailTemplateModel
    {
        PopulateDefaultBaseData(model);
        var body = await _templateEngine.RenderTemplateAsync(templateName, model);

        var request = new EmailRequest
        {
            To = new() { to },
            Subject = $"{subject} - {model.AppName}",
            Body = body,
            IsHtml = true,
            SenderDisplayName = model.AppName
        };

        await _emailSender.SendEmailAsync(request);
    }

    private void PopulateDefaultBaseData(BaseEmailTemplateModel model)
    {
        // تغيير الإعدادات الافتراضية لتناسب مشروع Skill Loop
        model.AppName = string.IsNullOrWhiteSpace(model.AppName) ? AppName : model.AppName;
        model.SupportEmail = string.IsNullOrWhiteSpace(model.SupportEmail) ? "support@skillloop.com" : model.SupportEmail;
        // افتراضياً، قم بتغيير الروابط بناءً على المشروع
        model.WebsiteUrl = string.IsNullOrWhiteSpace(model.WebsiteUrl) ? _baseUrlOptions.Frontend : model.WebsiteUrl;
        model.FacebookUrl = string.IsNullOrWhiteSpace(model.FacebookUrl) ? "https://facebook.com/skillloop" : model.FacebookUrl;
        model.InstagramUrl = string.IsNullOrWhiteSpace(model.InstagramUrl) ? "https://instagram.com/skillloop" : model.InstagramUrl;
        model.ContactPhoneNumber = string.IsNullOrWhiteSpace(model.ContactPhoneNumber) ? "+201000000000" : model.ContactPhoneNumber;
    }
}