using Skill_Loop.Domain.Entities.Chat;

namespace Skill_Loop.Application.Features.Chat.DTOs;

/// <summary>
/// الرسالة زي ما بتوصل للـ Client (سواء من REST أو لحظياً عن طريق SignalR).
/// </summary>
public sealed record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    DateTime SentAt,
    DateTime? ReadAt);

/// <summary>
/// صف واحد في قايمة المحادثات.
/// </summary>
public sealed record ConversationDto(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    string? OtherUserAvatarUrl,
    string? LastMessagePreview,
    DateTime? LastMessageAt,
    int UnreadCount,
    string OtherUserRole = "Student");

/// <summary>
/// بيتبعت للمُرسِل لما الطرف التاني يقرا رسايله.
/// </summary>
public sealed record MessagesReadDto(Guid ConversationId, Guid ReaderId, DateTime ReadAt);

public static class ChatMappings
{
    public static MessageDto ToDto(this ChatMessage message) => new(
        message.Id,
        message.ConversationId,
        message.SenderId,
        message.Content,
        message.SentAt,
        message.ReadAt);
}
