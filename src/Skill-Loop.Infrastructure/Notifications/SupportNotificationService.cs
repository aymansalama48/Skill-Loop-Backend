using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.Email;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Common.Abstractions.Settings;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.External.Email;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.Notifications;

public sealed class SupportNotificationService : ISupportRequestNotifier, ISupportAnswerNotifier
{
    private readonly IEmailSender _emailSender;
    private readonly EmailTemplateEngine _templateEngine;
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly BaseUrlOptions _baseUrlOptions;

    private const string AppName = "Skill Loop";

    public SupportNotificationService(
        IEmailSender emailSender,
        EmailTemplateEngine templateEngine,
        IOptions<BaseUrlOptions> baseUrlOptions,
        ISiteSettingsService siteSettingsService)
    {
        _emailSender = emailSender;
        _templateEngine = templateEngine;
        _baseUrlOptions = baseUrlOptions.Value;
        _siteSettingsService = siteSettingsService;
    }

    public async Task<Result> SendContactFormConfirmationAsync(string toEmail, string userName, string subject, string questionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _siteSettingsService.GetSettingsAsync();

            var model = new
            {
                UserName = userName,
                Subject = subject,
                QuestionId = questionId,
                SupportEmail = settings.SupportEmail ?? "support@skill-loop.com",
                WebsiteUrl = settings.WebsiteUrl ?? _baseUrlOptions.Frontend,
                ContactPhoneNumber = settings.ContactPhoneNumber ?? string.Empty,
                WhatsAppNumber = settings.WhatsAppNumber ?? string.Empty,
                AppName = settings.AppName ?? AppName,
                CurrentYear = DateTime.UtcNow.Year
            };

            var htmlBody = await _templateEngine.RenderTemplateAsync("ContactFormConfirmation", model);

            var emailRequest = new EmailRequest
            {
                To = [toEmail],
                Subject = $"تم استلام استفسارك - {subject}",
                Body = htmlBody,
                IsHtml = true,
                SenderDisplayName = model.AppName
            };

            await _emailSender.SendEmailAsync(emailRequest);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("SupportNotification.EmailFailed", $"Failed to send confirmation email: {ex.Message}", ErrorType.Failure));
        }
    }

    public async Task<Result> SendSupportTeamNotificationAsync(string userName, string userEmail, string subject, string message, string category, string questionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _siteSettingsService.GetSettingsAsync();
            var supportEmail = settings.SupportEmail ?? "support@skill-loop.com";

            var model = new
            {
                UserName = userName,
                UserEmail = userEmail,
                Subject = subject,
                Message = message,
                Category = category,
                QuestionId = questionId,
                SubmittedAt = DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm"),
                AppName = settings.AppName ?? AppName
            };

            var htmlBody = await _templateEngine.RenderTemplateAsync("SupportNotification", model);

            var emailRequest = new EmailRequest
            {
                To = [supportEmail],
                Subject = $"استفسار جديد من {userName} - {subject}",
                Body = htmlBody,
                IsHtml = true,
                SenderDisplayName = model.AppName
            };

            await _emailSender.SendEmailAsync(emailRequest);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("SupportNotification.EmailFailed", $"Failed to send notification email: {ex.Message}", ErrorType.Failure));
        }
    }

    public async Task<Result> SendAnswerNotificationAsync(string toEmail, string userName, string question, string answer, string category, string questionId, CancellationToken cancellationToken = default)
    {
        return await SendAnswerEmailAsync(
            toEmail,
            userName,
            question,
            answer,
            category,
            questionId,
            intro: "يسعدنا إن فريق الدعم يرد على استفسارك، وفيه الرد موجود تحت 👇",
            subjectPrefix: "رد على استفسارك",
            cancellationToken);
    }

    public async Task<Result> SendFaqAnswerAsync(string toEmail, string userName, string question, string answer, string questionId, CancellationToken cancellationToken = default)
    {
        return await SendAnswerEmailAsync(
            toEmail,
            userName,
            question,
            answer,
            category: string.Empty,
            questionId,
            intro: "بناءً على طلبك، أرسلنا لك إجابة هذا السؤال من صفحة الأسئلة الشائعة.",
            subjectPrefix: "إجابة من الأسئلة الشائعة",
            cancellationToken);
    }

    private async Task<Result> SendAnswerEmailAsync(
        string toEmail,
        string userName,
        string question,
        string answer,
        string category,
        string questionId,
        string intro,
        string subjectPrefix,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return Result.Failure(new Error("SupportNotification.MissingRecipient", "لا يوجد بريد إلكتروني لإرسال الرد إليه.", ErrorType.Validation));
        }

        try
        {
            var settings = await _siteSettingsService.GetSettingsAsync();

            var model = new
            {
                UserName = string.IsNullOrWhiteSpace(userName) ? toEmail : userName,
                Question = question,
                Answer = answer,
                Category = category,
                QuestionId = questionId,
                Intro = intro,
                SupportEmail = settings.SupportEmail ?? "support@skill-loop.com",
                WebsiteUrl = settings.WebsiteUrl ?? _baseUrlOptions.Frontend,
                ContactPhoneNumber = settings.ContactPhoneNumber ?? string.Empty,
                WhatsAppNumber = settings.WhatsAppNumber ?? string.Empty,
                AppName = settings.AppName ?? AppName,
                CurrentYear = DateTime.UtcNow.Year
            };

            var htmlBody = await _templateEngine.RenderTemplateAsync("AnswerNotification", model);

            var emailRequest = new EmailRequest
            {
                To = [toEmail],
                Subject = $"{subjectPrefix} - #{questionId}",
                Body = htmlBody,
                IsHtml = true,
                SenderDisplayName = model.AppName
            };

            await _emailSender.SendEmailAsync(emailRequest);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("SupportNotification.EmailFailed", $"Failed to send answer email: {ex.Message}", ErrorType.Failure));
        }
    }
}
