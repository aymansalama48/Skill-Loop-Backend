using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Chat;

public static class ChatErrors
{
    public static readonly Error InvalidConversation = new Error(
        "Chat.InvalidConversation",
        "Chat is required.",
        ErrorType.Validation);

    public static readonly Error InvalidSender = new Error(
        "Chat.InvalidSender",
        "Chat is required.",
        ErrorType.Validation);

    public static readonly Error EmptyMessage = new Error(
        "Chat.EmptyMessage",
        "Chat empty message.",
        ErrorType.Validation);

    public static readonly Error InvalidParticipant = new Error(
        "Chat.InvalidParticipant",
        "Chat is required.",
        ErrorType.Validation);

    public static readonly Error SelfConversation = new Error(
        "Chat.SelfConversation",
        "Chat self conversation.",
        ErrorType.Validation);

}
