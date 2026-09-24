namespace Skill_Loop.Api.Hubs;

/// <summary>
/// أسماء الأحداث اللي السيرفر بيبعتها للـ Client. الـ Frontend لازم يسمع بنفس الأسماء.
/// </summary>
public static class ChatEvents
{
    /// <summary>وصلتلك رسالة جديدة (Payload: MessageDto)</summary>
    public const string ReceiveMessage = "ReceiveMessage";

    /// <summary>الطرف التاني قرا رسايلك (Payload: MessagesReadDto)</summary>
    public const string MessagesRead = "MessagesRead";
}
