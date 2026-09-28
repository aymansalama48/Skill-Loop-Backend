using System.Text.Json;
using MediatR;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Entities.Chat.Events;
using Skill_Loop.Domain.Entities.Notifications;

namespace Skill_Loop.Application.Features.Chat.EventHandlers.ChatMessageSent;

/// <summary>
/// بيتنفّذ من الـ Outbox Job (كل 5 ثواني) بعد ما رسالة شات جديدة تتحفظ.
/// شغله: يحوّل الرسالة لإشعار جوه التطبيق (In-App) للطرف التاني.
/// مفيش أي اتصال خارجي هنا (مفيش Firebase) — بس صف جديد في جدول Notifications.
/// </summary>
public sealed class ChatMessageSentEventHandler(
    IApplicationDbContext context,
    IUserManagementService userManagement)
    : INotificationHandler<DomainEventNotification<ChatMessageSentEvent>>
{
    private const int PreviewLength = 80;

    public async Task Handle(DomainEventNotification<ChatMessageSentEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var conversation = await context.FirstOrDefaultAsync(
            context.AsNoTracking(context.Conversations.Where(c => c.Id == domainEvent.ConversationId)),
            cancellationToken);

        // المحادثة أو الرسالة اتمسحت قبل ما نوصل هنا؟ سيناريو نادر جداً، تجاهل بأمان.
        if (conversation is null)
            return;

        var message = await context.FirstOrDefaultAsync(
            context.AsNoTracking(context.ChatMessages.Where(m => m.Id == domainEvent.MessageId)),
            cancellationToken);

        if (message is null)
            return;

        var recipientId = conversation.GetOtherParticipantId(domainEvent.SenderId);

        var senderResult = await userManagement.GetByIdAsync(domainEvent.SenderId, cancellationToken);
        var senderName = senderResult.IsSuccess ? senderResult.Data!.FullName : "مستخدم";

        var preview = message.Content.Length <= PreviewLength
            ? message.Content
            : message.Content[..PreviewLength] + "…";

        var data = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["conversationId"] = domainEvent.ConversationId.ToString(),
            ["senderId"] = domainEvent.SenderId.ToString()
        });

        var createResult = Notification.Create(recipientId, "ChatMessage", senderName, preview, data);
        if (createResult.IsFailure)
            return; // مينفعش نفشّل الـ Outbox كله بسبب إشعار

        context.Add(createResult.Data!);
        await context.SaveChangesAsync(cancellationToken);
    }
}
