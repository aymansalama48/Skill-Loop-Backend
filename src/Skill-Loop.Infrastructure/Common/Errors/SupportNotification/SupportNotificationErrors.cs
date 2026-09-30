using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Infrastructure.Common.Errors.SupportNotification;

public static class SupportNotificationErrors
{
    public static readonly Error MissingRecipient = new Error(
        "SupportNotification.MissingRecipient",
        "SupportNotification missing recipient.",
        ErrorType.Validation);

}
