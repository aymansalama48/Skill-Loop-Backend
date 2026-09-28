using Skill_Loop.Domain.Entities.Notifications;

namespace Skill_Loop.Application.Features.Notifications.DTOs;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? Data,
    bool IsRead,
    DateTime CreatedAt);

public static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification notification) => new(
        notification.Id,
        notification.Type,
        notification.Title,
        notification.Body,
        notification.Data,
        notification.IsRead,
        notification.CreatedAt);
}
