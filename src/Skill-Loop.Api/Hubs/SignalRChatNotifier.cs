using Microsoft.AspNetCore.SignalR;
using Skill_Loop.Application.Common.Abstractions.External.Realtime;
using Skill_Loop.Application.Features.Chat.DTOs;

namespace Skill_Loop.Api.Hubs;

/// <summary>
/// التنفيذ الحقيقي لـ IChatNotifier باستخدام SignalR.
/// IHubContext هو الطريقة اللي نبعت بيها لعملاء الـ Hub من "بره" الـ Hub (من Handler مثلاً).
/// </summary>
public sealed class SignalRChatNotifier(
    IHubContext<ChatHub> hubContext,
    ILogger<SignalRChatNotifier> logger) : IChatNotifier
{
    public async Task NotifyMessageReceivedAsync(Guid recipientUserId, MessageDto message, CancellationToken cancellationToken = default)
    {
        try
        {
            // Clients.User(id) = كل الأجهزة/التابات المفتوحة للمستخدم ده
            await hubContext.Clients.User(recipientUserId.ToString())
                .SendAsync(ChatEvents.ReceiveMessage, message, cancellationToken);
        }
        catch (Exception ex)
        {
            // الرسالة اتحفظت خلاص. فشل الإشعار اللحظي مينفعش يبوّظ العملية.
            logger.LogWarning(ex, "Failed to push chat message {MessageId} to user {UserId}", message.Id, recipientUserId);
        }
    }

    public async Task NotifyMessagesReadAsync(Guid recipientUserId, MessagesReadDto payload, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.User(recipientUserId.ToString())
                .SendAsync(ChatEvents.MessagesRead, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to push read receipt for conversation {ConversationId} to user {UserId}",
                payload.ConversationId, recipientUserId);
        }
    }
}
