using Skill_Loop.Domain.Common.Entities;

namespace Skill_Loop.Domain.Entities.Emails;

public class EmailLog : BaseEntity
{
    private EmailLog(
        string type,
        string recipientEmail,
        string? referenceId,
        string subject,
        string body)
    {
        Id = Guid.CreateVersion7();
        Type = type;
        RecipientEmail = recipientEmail;
        ReferenceId = referenceId;
        Subject = subject;
        Body = body;
        Status = EmailStatus.Pending;
        Attempts = 0;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private EmailLog() { }

    public string Type { get; private set; } = null!;
    public string RecipientEmail { get; private set; } = null!;
    public string? ReferenceId { get; private set; }
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public EmailStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }

    public static EmailLog Create(
        string type,
        string recipientEmail,
        string? referenceId,
        string subject,
        string body)
    {
        return new EmailLog(type, recipientEmail, referenceId, subject, body);
    }

    public void MarkAsSent(DateTime sentAtUtc)
    {
        Status = EmailStatus.Sent;
        SentAtUtc = sentAtUtc;
        Attempts++;
    }

    public void MarkAsFailed(string error)
    {
        Status = EmailStatus.Failed;
        LastError = error;
        Attempts++;
    }
}
