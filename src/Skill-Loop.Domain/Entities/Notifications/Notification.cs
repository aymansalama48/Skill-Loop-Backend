using Skill_Loop.Domain.Common.Errors.Notification;
using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Entities.Notifications;

/// <summary>
/// إشعار جوه التطبيق (In-App). بيتخزن في الداتابيز ويظهر في شاشة "الإشعارات"
/// لما المستخدم يفتح التطبيق. مفيش أي اتصال خارجي (Firebase/SMS/إلخ).
/// </summary>
public sealed class Notification : AuditableEntity
{
    public const int TitleMaxLength = 200;
    public const int BodyMaxLength = 500;

    /// <summary>لمين الإشعار ده.</summary>
    public Guid UserId { get; private set; }

    /// <summary>نوع الإشعار، مثلاً "ChatMessage". بيساعد الفرونت يعرف يعمل إيه لما يتضغط.</summary>
    public string Type { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// بيانات إضافية (JSON) زي conversationId، عشان لو المستخدم دوس على الإشعار
    /// يروح للمكان الصح في التطبيق. اختياري.
    /// </summary>
    public string? Data { get; private set; }

    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public static Result<Notification> Create(Guid userId, string type, string title, string body, string? data = null)
    {
        if (userId == Guid.Empty)
            return Result<Notification>.Failure(NotificationErrors.InvalidUser);

        if (string.IsNullOrWhiteSpace(type))
            return Result<Notification>.Failure(NotificationErrors.InvalidType);

        var trimmedTitle = (title ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmedTitle))
            return Result<Notification>.Failure(NotificationErrors.EmptyTitle);

        // قص دفاعي بدل ما نرفض الإشعار كامل لو النص طلع أطول من المتوقع بقليل
        if (trimmedTitle.Length > TitleMaxLength)
            trimmedTitle = trimmedTitle[..TitleMaxLength];

        var trimmedBody = (body ?? string.Empty).Trim();
        if (trimmedBody.Length > BodyMaxLength)
            trimmedBody = trimmedBody[..BodyMaxLength];

        return Result<Notification>.Success(new Notification
        {
            UserId = userId,
            Type = type,
            Title = trimmedTitle,
            Body = trimmedBody,
            Data = data,
            IsRead = false
        });
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
