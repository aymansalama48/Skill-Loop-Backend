using Skill_Loop.Domain.Common.Events;

namespace Skill_Loop.Domain.Entities.Chat.Events;

/// <summary>
/// بترفعه رسالة الشات بعد ما تتحفظ. الـ Handler في الـ Application هو اللي
/// بيحوّلها لإشعار جوه التطبيق (In-App Notification) للطرف التاني.
/// </summary>
public sealed record ChatMessageSentEvent(Guid MessageId, Guid ConversationId, Guid SenderId) : IDomainEvent;
