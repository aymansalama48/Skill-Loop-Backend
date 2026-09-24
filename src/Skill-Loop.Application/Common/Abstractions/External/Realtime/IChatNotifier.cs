using Skill_Loop.Application.Features.Chat.DTOs;

namespace Skill_Loop.Application.Common.Abstractions.External.Realtime;

/// <summary>
/// عقد "إشعار الشات اللحظي". طبقة الـ Application بتعرف إن فيه حاجة اسمها Notifier
/// بس مش بتعرف إنه SignalR ولا غيره. التنفيذ الحقيقي في طبقة الـ Api.
/// </summary>
public interface IChatNotifier
{
    /// <summary>
    /// ابعت الرسالة الجديدة للمستقبِل فوراً (لو Online).
    /// </summary>
    Task NotifyMessageReceivedAsync(
        Guid recipientUserId,
        MessageDto message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// قول للمُرسِل إن رسايله اتقرت.
    /// </summary>
    Task NotifyMessagesReadAsync(
        Guid recipientUserId,
        MessagesReadDto payload,
        CancellationToken cancellationToken = default);
}
