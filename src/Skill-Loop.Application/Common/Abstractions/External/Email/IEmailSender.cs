using Skill_Loop.Application.Common.Abstractions.External.Email.Models;

namespace Skill_Loop.Application.Common.Abstractions.External.Email;

/// <summary>
/// واجهة إرسال البريد الأساسية (Pure Infrastructure Service)
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// إرسال بريد إلكتروني مخصص
    /// </summary>
    Task SendEmailAsync(EmailRequest request);
}