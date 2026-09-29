using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Chat.Events;

namespace Skill_Loop.Domain.Entities.Chat;

/// <summary>
/// رسالة واحدة جوه محادثة. بنستخدم BaseEntity (من غير Audit) لأن الرسائل كتير
/// وعندنا SentAt بنحطه بنفسنا.
/// </summary>
public sealed class ChatMessage : BaseEntity
{
    public const int MaxContentLength = 2000;

    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }

    /// <summary>
    /// وقت ما المستقبِل قرأ الرسالة. null = لسه ما اتقراتش.
    /// </summary>
    public DateTime? ReadAt { get; private set; }

    // EF Core محتاج constructor فاضي
    private ChatMessage() { }

    public static Result<ChatMessage> Create(Guid conversationId, Guid senderId, string? content)
    {
        if (conversationId == Guid.Empty)
            return Result<ChatMessage>.Failure(new Error("Chat.InvalidConversation", "المحادثة مطلوبة.", ErrorType.Validation));

        if (senderId == Guid.Empty)
            return Result<ChatMessage>.Failure(new Error("Chat.InvalidSender", "المرسل مطلوب.", ErrorType.Validation));

        var trimmed = content?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            return Result<ChatMessage>.Failure(new Error("Chat.EmptyMessage", "لا يمكن إرسال رسالة فارغة.", ErrorType.Validation));

        if (trimmed.Length > MaxContentLength)
            return Result<ChatMessage>.Failure(new Error(
                "Chat.MessageTooLong",
                $"الرسالة أطول من الحد المسموح ({MaxContentLength} حرف).",
                ErrorType.Validation));

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Content = trimmed,
            SentAt = DateTime.UtcNow
        };

        // 👈 هنا بنرفع الحدث. الـ Outbox هيلقطه لوحده وقت الحفظ (زي StaffInvitation بالظبط)
        message.AddDomainEvent(new ChatMessageSentEvent(message.Id, conversationId, senderId));

        return Result<ChatMessage>.Success(message);
    }

    /// <summary>
    /// علّم الرسالة إنها اتقراتش. لو اتقراتش قبل كده مش بنغير الوقت (idempotent).
    /// </summary>
    public void MarkAsRead() => ReadAt ??= DateTime.UtcNow;
}
