using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Entities.Chat;

/// <summary>
/// محادثة 1-لـ-1 بين مستخدمين اتنين (مثلاً طالب ومعلم).
/// بنخزّن آخر رسالة (Preview) جوه المحادثة نفسها عشان شاشة "قايمة المحادثات"
/// تتحمل بسرعة من غير ما نجيب كل الرسائل.
/// </summary>
public sealed class Conversation : AuditableEntity
{
    public const int PreviewMaxLength = 100;

    // ⚠️ الترتيب ثابت: ParticipantOneId دايماً هو الـ Guid الأصغر.
    // ده بيخلّي (أحمد ↔ منى) و (منى ↔ أحمد) نفس الصف بالظبط،
    // والـ Unique Index في الداتابيز يمنع تكرار المحادثة.
    public Guid ParticipantOneId { get; private set; }
    public Guid ParticipantTwoId { get; private set; }

    public DateTime? LastMessageAt { get; private set; }
    public string? LastMessagePreview { get; private set; }

    private Conversation() { }

    /// <summary>
    /// بيرتّب الـ Guid-ين بنفس القاعدة اللي بنخزّن بيها (الأصغر الأول).
    /// بنستخدمها كمان في البحث عن محادثة موجودة.
    /// </summary>
    public static (Guid One, Guid Two) NormalizePair(Guid a, Guid b)
        => a.CompareTo(b) <= 0 ? (a, b) : (b, a);

    public static Result<Conversation> Create(Guid userId, Guid otherUserId)
    {
        if (userId == Guid.Empty || otherUserId == Guid.Empty)
            return Result<Conversation>.Failure(new Error("Chat.InvalidParticipant", "المستخدم مطلوب.", ErrorType.Validation));

        if (userId == otherUserId)
            return Result<Conversation>.Failure(new Error("Chat.SelfConversation", "لا يمكنك بدء محادثة مع نفسك.", ErrorType.Validation));

        var (one, two) = NormalizePair(userId, otherUserId);

        return Result<Conversation>.Success(new Conversation
        {
            ParticipantOneId = one,
            ParticipantTwoId = two
        });
    }

    public bool HasParticipant(Guid userId)
        => userId == ParticipantOneId || userId == ParticipantTwoId;

    /// <summary>
    /// مين الطرف التاني في المحادثة؟ (بنستخدمها عشان نعرف نبعت الإشعار لمين)
    /// </summary>
    public Guid GetOtherParticipantId(Guid userId)
        => userId == ParticipantOneId ? ParticipantTwoId : ParticipantOneId;

    /// <summary>
    /// بتتنادي لما رسالة جديدة تتبعت، عشان نحدّث "آخر رسالة".
    /// </summary>
    public void RegisterMessage(ChatMessage message)
    {
        LastMessageAt = message.SentAt;

        var preview = message.Content.Length <= PreviewMaxLength
            ? message.Content
            : message.Content[..PreviewMaxLength];

        // نتجنب إننا نقص الإيموجي من النص (بيتكوّن من حرفين)
        if (preview.Length > 0 && char.IsHighSurrogate(preview[^1]))
            preview = preview[..^1];

        LastMessagePreview = preview;
    }
}
