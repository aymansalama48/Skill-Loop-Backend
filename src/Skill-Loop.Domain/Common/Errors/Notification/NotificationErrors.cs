using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Notification;

public static class NotificationErrors
{
    public static readonly Error InvalidUser = new Error(
        "Notification.InvalidUser",
        "Notification is required.",
        ErrorType.Validation);

    public static readonly Error InvalidType = new Error(
        "Notification.InvalidType",
        "Notification is required.",
        ErrorType.Validation);

    public static readonly Error EmptyTitle = new Error(
        "Notification.EmptyTitle",
        "Notification is required.",
        ErrorType.Validation);

}
