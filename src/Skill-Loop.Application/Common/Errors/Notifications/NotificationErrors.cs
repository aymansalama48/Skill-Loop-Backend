using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Notifications;

public static class NotificationErrors
{
    public static readonly Error NotFound = new(
        "NOTIFICATION_NOT_FOUND",
        "الإشعار غير موجود.",
        ErrorType.NotFound);
}
