using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Chat;

/// <summary>
/// أخطاء الشات على مستوى الـ Application (أخطاء الـ Domain بتكون جوه الكيانات نفسها).
/// </summary>
public static class ChatErrors
{
    public static readonly Error ConversationNotFound = new(
        "CHAT_CONVERSATION_NOT_FOUND",
        "المحادثة غير موجودة.",
        ErrorType.NotFound);

    public static readonly Error NotParticipant = new(
        "CHAT_NOT_PARTICIPANT",
        "لا يمكنك الوصول إلى هذه المحادثة.",
        ErrorType.Forbidden);

    public static readonly Error CannotChatWithSelf = new(
        "CHAT_CANNOT_CHAT_WITH_SELF",
        "لا يمكنك بدء محادثة مع نفسك.",
        ErrorType.Validation);

    public static readonly Error RecipientUnavailable = new(
        "CHAT_RECIPIENT_UNAVAILABLE",
        "هذا المستخدم غير متاح للمحادثة حالياً.",
        ErrorType.Forbidden);
}
