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
using Skill_Loop.Application.Common.Abstractions.Settings;

using Skill_Loop.Infrastructure.Options;
using System.Reflection;

namespace Skill_Loop.Infrastructure.Notifications;

public sealed class IdentityNotificationService : IIdentityNotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateEngine _templateEngine;
    private readonly BaseUrlOptions _baseUrlOptions;
    private readonly IDateTime _dateTimeProvider;
    private readonly IUserAgentParser _userAgentParser;
    private readonly IGeoLocationService _geoLocationService;
    private readonly ISiteSettingsService _siteSettingsService;

    private const string AppName = "Skill Loop";

    public IdentityNotificationService(
        IEmailSender emailSender,
        IEmailTemplateEngine templateEngine,
        IOptions<BaseUrlOptions> baseUrlOptions,
        IDateTime dateTimeProvider,
        IUserAgentParser userAgentParser,
        IGeoLocationService geoLocationService,
        ISiteSettingsService siteSettingsService)
    {
        _emailSender = emailSender;
        _templateEngine = templateEngine;
        _baseUrlOptions = baseUrlOptions.Value;
        _dateTimeProvider = dateTimeProvider;
        _userAgentParser = userAgentParser;
        _geoLocationService = geoLocationService;
        _siteSettingsService = siteSettingsService;
    }

    // ============================================================
    // 1) Login
    // ============================================================
    public async Task SendLoginEmailAsync(
        string email,
        LoginTemplateModel model,
        string? ipAddress = null,
        string? userAgent = null)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName)
            ? "المستخدم العزيز"
            : model.UserName;

        if (model.LoginTime == default)
            model.LoginTime = _dateTimeProvider.Now;

        await PopulateClientInfoAsync(model, userAgent, ipAddress);

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.Login,
            EmailTemplateNames.Login,
            model);
    }

    // ============================================================
    // 2) Email Confirmation
    // ============================================================
    public async Task SendEmailConfirmationAsync(
        string email,
        EmailConfirmationTemplateModel model,
        string? ipAddress = null,
        string? userAgent = null)
    {
        await PopulateClientInfoAsync(model, userAgent, ipAddress);

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.EmailConfirmation,
            EmailTemplateNames.EmailConfirmation,
            model);
    }

    // ============================================================
    // 3) Reset Password
    // ============================================================
    public async Task SendResetPasswordEmailAsync(
        string email,
        ResetPasswordTemplateModel model,
        string? ipAddress = null,
        string? userAgent = null)
    {
        await PopulateClientInfoAsync(model, userAgent, ipAddress);

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.ResetPassword,
            EmailTemplateNames.ResetPassword,
            model);
    }

    // ============================================================
    // 4) Welcome
    // ============================================================
    public async Task SendWelcomeEmailAsync(
        string email,
        WelcomeTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName)
            ? "المستخدم العزيز"
            : model.UserName;

        model.LoginUrl = string.IsNullOrWhiteSpace(model.LoginUrl)
            ? $"{_baseUrlOptions.Frontend}/login"
            : model.LoginUrl;

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.Welcome,
            EmailTemplateNames.Welcome,
            model);
    }

    // ============================================================
    // 5) Password Changed
    // ============================================================
    public async Task SendPasswordChangedEmailAsync(
        string email,
        PasswordChangedTemplateModel model,
        string? ipAddress = null,
        string? userAgent = null)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName)
            ? "المستخدم العزيز"
            : model.UserName;

        if (model.ChangedAt == default)
            model.ChangedAt = _dateTimeProvider.Now;

        await PopulateClientInfoAsync(model, userAgent, ipAddress);

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.PasswordChanged,
            EmailTemplateNames.PasswordChanged,
            model);
    }

    // ============================================================
    // 6) Account Locked
    // ============================================================
    public async Task SendAccountLockedEmailAsync(
        string email,
        AccountLockedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName)
            ? "المستخدم العزيز"
            : model.UserName;

        model.UnlockUrl = string.IsNullOrWhiteSpace(model.UnlockUrl)
            ? $"{_baseUrlOptions.Frontend}/support"
            : model.UnlockUrl;

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.AccountLocked,
            EmailTemplateNames.AccountLocked,
            model);
    }

    // ============================================================
    // 7) Account Unlocked
    // ============================================================
    public async Task SendAccountUnlockedEmailAsync(
        string email,
        AccountUnlockedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName)
            ? "المستخدم العزيز"
            : model.UserName;

        model.LoginUrl = string.IsNullOrWhiteSpace(model.LoginUrl)
            ? $"{_baseUrlOptions.Frontend}/login"
            : model.LoginUrl;

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.AccountUnlocked,
            EmailTemplateNames.AccountUnlocked,
            model);
    }

    // ============================================================
    // 8) Role Assigned
    // ============================================================
    public async Task SendRoleAssignedEmailAsync(
        string email,
        RoleAssignedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName)
            ? "المستخدم العزيز"
            : model.UserName;

        model.RoleName = string.IsNullOrWhiteSpace(model.RoleName)
            ? "مستخدم"
            : model.RoleName;

        model.DashboardUrl = string.IsNullOrWhiteSpace(model.DashboardUrl)
            ? $"{_baseUrlOptions.Frontend}/dashboard"
            : model.DashboardUrl;

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.RoleAssigned,
            EmailTemplateNames.RoleAssigned,
            model);
    }

    // ============================================================
    // 9) Role Removed
    // ============================================================
    public async Task SendRoleRemovedEmailAsync(
        string email,
        RoleRemovedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName)
            ? "المستخدم العزيز"
            : model.UserName;

        model.RoleName = string.IsNullOrWhiteSpace(model.RoleName)
            ? "مستخدم"
            : model.RoleName;

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.RoleRemoved,
            EmailTemplateNames.RoleRemoved,
            model);
    }

    // ============================================================
    // 10) Staff Invitation
    // ============================================================
    public async Task SendStaffInvitationEmailAsync(
        string email,
        StaffInvitationTemplateModel model)
    {
        model.RoleName = string.IsNullOrWhiteSpace(model.RoleName)
            ? "موظف"
            : model.RoleName;

        model.AdminName = string.IsNullOrWhiteSpace(model.AdminName)
            ? "إدارة النظام"
            : model.AdminName;

        model.InvitedEmail = string.IsNullOrWhiteSpace(model.InvitedEmail)
            ? email
            : model.InvitedEmail;

        if (string.IsNullOrWhiteSpace(model.InvitationLink)
            && !string.IsNullOrWhiteSpace(model.Token))
        {
            model.InvitationLink =
                $"{_baseUrlOptions.Frontend}/accept-invitation?token={model.Token}";
        }

        await SendTemplateEmailAsync(
            email,
            EmailSubjects.StaffInvitation,
            EmailTemplateNames.StaffInvitation,
            model);
    }

    // ============================================================
    // 11) Session Material Uploaded (إشعار للمستخدم)
    // ============================================================
    public async Task SendMaterialUploadedEmailAsync(
        string email,
        SessionMaterialUploadedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName)
            ? email
            : model.UserName;

        // ✅ الـ subject factory بياخد اسم التطبيق من الـ Service بعد ما يتحدد
        await SendTemplateEmailAsync(
            email,
            companyName => EmailSubjects.MaterialUploaded(model.MaterialName, companyName),
            EmailTemplateNames.SessionMaterialUploaded,
            model);
    }

    // ============================================================
    // 12) Session Material Uploaded — Confirmation (للمعلم/المشرف)
    // ============================================================
    public async Task SendMaterialUploadedConfirmationAsync(
        string email,
        SessionMaterialUploadedTemplateModel model)
    {
        model.UserName = string.IsNullOrWhiteSpace(model.UserName)
            ? email
            : model.UserName;

        await SendTemplateEmailAsync(
            email,
            companyName => EmailSubjects.MaterialUploadedConfirmation(model.MaterialName, companyName),
            EmailTemplateNames.SessionMaterialUploaded,
            model);
    }

    // ============================================================
    // 13) Storage Quota Warning (تنبيه إداري)
    // ============================================================
    public async Task SendStorageQuotaWarningAsync(
        long usedBytes,
        long totalBytes,
        double threshold)
    {
        var model = new StorageQuotaWarningTemplateModel
        {
            UsedBytes = usedBytes,
            TotalBytes = totalBytes,
            Threshold = threshold,
            UsedFormatted = FormatBytes(usedBytes),
            TotalFormatted = FormatBytes(totalBytes),
        };

        // ✅ نسحب إيميل الأدمن من الإعدادات، مع fallback
        var settings = await _siteSettingsService.GetSettingsAsync();
        var adminEmail = !string.IsNullOrWhiteSpace(settings.SupportEmail)
            ? settings.SupportEmail
            : "support@skillloop.com";

        await SendTemplateEmailAsync(
            adminEmail,
            EmailSubjects.StorageQuotaWarning,
            EmailTemplateNames.StorageQuotaWarning,
            model);
    }

    // ============================================================
    // Helper: Format Bytes
    // ============================================================
    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1024L * 1024 * 1024)
            return $"{(bytes / (1024.0 * 1024 * 1024)):F2} GB";

        if (bytes >= 1024L * 1024)
            return $"{(bytes / (1024.0 * 1024)):F2} MB";

        if (bytes >= 1024L)
            return $"{(bytes / 1024.0):F2} KB";

        return $"{bytes} B";
    }

    // ============================================================
    // Helper: Populate Client Info (IP / Device / Location)
    // ============================================================

    /// <summary>
    /// تعبئة بيانات العميل (IP، الجهاز، الموقع) على الـ Model.
    /// القيم غير المتوفرة تتركها فاضية (string.Empty) — القالب يخفي الحقل تلقائياً
    /// عبر {{#if Property}}.
    ///
    /// ⚠️ ملاحظة معمارية: الخدمة بتشتغل في Hangfire Worker Thread،
    /// فما ينفعش نحقن IClientContext / IHttpContextAccessor هنا.
    /// الـ ipAddress و userAgent بيتمرروا كـ Parameters من المستدعي (في HTTP Request).
    /// </summary>
    private async Task PopulateClientInfoAsync<TModel>(
        TModel model,
        string? userAgent,
        string? ipAddress)
        where TModel : class
    {
        // القيم الافتراضية: فاضية، مش "غير معروف"
        SetPropertyValue(model, "IpAddress", ipAddress ?? string.Empty);
        SetPropertyValue(model, "Device", string.Empty);
        SetPropertyValue(model, "Location", string.Empty);

        // Device من الـ User Agent
        try
        {
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                var deviceInfo = _userAgentParser.Parse(userAgent);
                var deviceName = BuildDeviceName(deviceInfo);

                if (!string.IsNullOrWhiteSpace(deviceName))
                {
                    SetPropertyValue(model, "Device", deviceName);
                }
            }
        }
        catch
        {
            // نتجاهل أخطاء الـ Parsing — الحقل يبقى فاضي والقالب يخفيه
        }

        // Location من الـ IP
        try
        {
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                var location = await _geoLocationService
                    .GetLocationAsync(ipAddress, CancellationToken.None);

                if (!string.IsNullOrWhiteSpace(location))
                {
                    SetPropertyValue(model, "Location", location);
                }
            }
        }
        catch
        {
            // نتجاهل أخطاء الـ Geo Lookup
        }
    }

    /// <summary>
    /// تعيين قيمة خاصية على الـ Model بالـ Reflection
    /// (لو الخاصية موجودة وقابلة للكتابة).
    /// </summary>
    private static void SetPropertyValue<TModel>(
        TModel model,
        string propertyName,
        string value)
        where TModel : class
    {
        if (model is null) return;

        var prop = typeof(TModel).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        if (prop is not null && prop.CanWrite)
        {
            prop.SetValue(model, value);
        }
    }

    /// <summary>
    /// بناء اسم الجهاز من الـ Browser و OperatingSystem.
    /// يرجّع string.Empty لو مفيش بيانات مفيدة.
    /// </summary>
    private static string BuildDeviceName(ClientDeviceInfo device)
    {
        var browser = device.Browser?.Trim();
        var operatingSystem = device.OperatingSystem?.Trim();

        if (string.IsNullOrWhiteSpace(browser)
            && string.IsNullOrWhiteSpace(operatingSystem))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(browser))
            return operatingSystem!;

        if (string.IsNullOrWhiteSpace(operatingSystem))
            return browser!;

        return $"{browser} on {operatingSystem}";
    }

    // ============================================================
    // Helper: Send Template Email
    // ============================================================

    /// <summary>
    /// تجهيز الـ Model بالبيانات الافتراضية، رندر القالب، ثم إرسال الإيميل.
    ///
    /// ⚠️ ملاحظة مهمة على subjectFactory:
    /// الباراميتر دالة بتاخد اسم التطبيق وترجّع العنوان النهائي كاملاً.
    /// السبب: عناوين EmailSubjects أصلاً بتحتوي على اسم التطبيق
    /// (زي "تأكيد بريدك - Skill Loop")، فلو أضفنا اسم التطبيق هنا تاني
    /// هيطلع مكرر: "تأكيد بريدك - Skill Loop - Skill Loop".
    ///
    /// للحالات الديناميكية (زي MaterialName)، مرّر lambda:
    ///     companyName => EmailSubjects.MaterialUploaded(model.MaterialName, companyName)
    /// </summary>
    private async Task SendTemplateEmailAsync<TModel>(
        string to,
        Func<string, string> subjectFactory,
        string templateName,
        TModel model)
        where TModel : BaseEmailTemplateModel
    {
        // 1. نعبّي الحقول الأساسية (AppName, SupportEmail, WebsiteUrl, ...)
        await PopulateDefaultBaseDataAsync(model);

        // 2. نرندر جسم الإيميل من القالب
        var body = await _templateEngine.RenderTemplateAsync(templateName, model);

        // 3. نبني العنوان النهائي
        var finalSubject = subjectFactory(model.AppName);

        // 4. نبعت الإيميل
        var request = new EmailRequest
        {
            To = new() { to },
            Subject = finalSubject,
            Body = body,
            IsHtml = true,
            SenderDisplayName = model.AppName
        };

        await _emailSender.SendEmailAsync(request);
    }

    // ============================================================
    // Helper: Populate Default Base Data
    // ============================================================

    /// <summary>
    /// تعبئة الحقول الأساسية المشتركة من إعدادات الموقع (SiteSettings).
    /// لو حقل مش موجود في الإعدادات، يبقى فاضي — القالب يخفيه عبر {{#if}}.
    /// </summary>
    private async Task PopulateDefaultBaseDataAsync(BaseEmailTemplateModel model)
    {
        // نجيب إعدادات الموقع (مرة واحدة لكل إيميل)
        var settings = await _siteSettingsService.GetSettingsAsync();

        model.AppName = string.IsNullOrWhiteSpace(model.AppName)
            ? (!string.IsNullOrWhiteSpace(settings.AppName) ? settings.AppName : AppName)
            : model.AppName;

        model.SupportEmail = string.IsNullOrWhiteSpace(model.SupportEmail)
            ? (!string.IsNullOrWhiteSpace(settings.SupportEmail) ? settings.SupportEmail : "support@skillloop.com")
            : model.SupportEmail;

        model.WebsiteUrl = string.IsNullOrWhiteSpace(model.WebsiteUrl)
            ? (!string.IsNullOrWhiteSpace(settings.WebsiteUrl) ? settings.WebsiteUrl : _baseUrlOptions.Frontend)
            : model.WebsiteUrl;

        model.FacebookUrl = string.IsNullOrWhiteSpace(model.FacebookUrl)
            ? (settings.FacebookUrl ?? string.Empty)
            : model.FacebookUrl;

        model.InstagramUrl = string.IsNullOrWhiteSpace(model.InstagramUrl)
            ? (settings.InstagramUrl ?? string.Empty)
            : model.InstagramUrl;

        model.ContactPhoneNumber = string.IsNullOrWhiteSpace(model.ContactPhoneNumber)
            ? (settings.ContactPhoneNumber ?? string.Empty)
            : model.ContactPhoneNumber;

        // ✅ إضافة WhatsAppNumber — كانت ناقصة
        model.WhatsAppNumber = string.IsNullOrWhiteSpace(model.WhatsAppNumber)
            ? (settings.WhatsAppNumber ?? string.Empty)
            : model.WhatsAppNumber;
    }
}