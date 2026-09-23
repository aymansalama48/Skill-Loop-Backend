using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.External.Client;
using Skill_Loop.Application.Common.Abstractions.External.Client.Models;
using Skill_Loop.Application.Common.Abstractions.External.Email;
using Skill_Loop.Application.Common.Abstractions.External.Email.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.Base;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Settings;
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
    private readonly ISiteSettingsService _siteSettingsService;

    private const string AppName = "Skill Loop";

    public IdentityNotificationService(
        IEmailSender emailSender,
        EmailTemplateEngine templateEngine,
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
    // الدوال المساعدة
    // ============================================================

    private async Task PopulateClientInfoAsync<TModel>(
        TModel model,
        string? userAgent,
        string? ipAddress)
        where TModel : class
    {
        SetPropertyValue(model, "IpAddress", ipAddress ?? string.Empty);
        SetPropertyValue(model, "Device", string.Empty);
        SetPropertyValue(model, "Location", string.Empty);

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
        }

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
        }
    }

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

    private async Task SendTemplateEmailAsync<TModel>(
        string to,
        Func<string, string> subjectFactory,
        string templateName,
        TModel model)
        where TModel : BaseEmailTemplateModel
    {
        // استخدام Await هنا لانتظار البيانات من الداتابيز أو الكاش
        await PopulateDefaultBaseDataAsync(model);

        var body = await _templateEngine.RenderTemplateAsync(templateName, model);

        var finalSubject = subjectFactory(model.AppName);

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

    private async Task PopulateDefaultBaseDataAsync(BaseEmailTemplateModel model)
    {
        // استدعاء إعدادات الموقع
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
            ? settings.FacebookUrl ?? string.Empty
            : model.FacebookUrl;

        model.InstagramUrl = string.IsNullOrWhiteSpace(model.InstagramUrl)
            ? settings.InstagramUrl ?? string.Empty
            : model.InstagramUrl;

        model.ContactPhoneNumber = string.IsNullOrWhiteSpace(model.ContactPhoneNumber)
            ? settings.ContactPhoneNumber ?? string.Empty
            : model.ContactPhoneNumber;
    }
}