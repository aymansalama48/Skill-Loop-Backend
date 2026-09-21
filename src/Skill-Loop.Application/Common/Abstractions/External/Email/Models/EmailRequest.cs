namespace Skill_Loop.Application.Common.Abstractions.External.Email.Models;

public class EmailRequest
{
    public List<string> To { get; set; } = new();
    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();

    // 🆕 اسم الراسل الديناميكي (مثل اسم الشركة)
    public string? SenderDisplayName { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<EmailAttachment> Attachments { get; set; } = new();
}
