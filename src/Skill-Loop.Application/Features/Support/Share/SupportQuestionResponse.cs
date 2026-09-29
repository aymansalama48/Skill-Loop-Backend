namespace Skill_Loop.Application.Features.Support.Share;

/// <summary>
/// شكل السؤال المستخدم في الـ FAQ العام.
/// لا يحتوي على بريد المستخدم ولا اسمه — ده Endpoint عام (Anonymous).
/// </summary>
public sealed record SupportQuestionResponse(
    Guid Id,
    string Question,
    string? Answer,
    string Category,
    bool IsAnswered,
    DateTime? AnsweredAt,
    DateTime CreatedAt);

/// <summary>
/// الشكل الكامل للسؤال — لفريق الدعم والـ Admin فقط.
/// </summary>
public sealed record SupportQuestionDetailsResponse(
    Guid Id,
    string Question,
    string? Answer,
    string Category,
    bool IsPublished,
    string? UserEmail,
    string? UserName,
    bool IsAnswered,
    DateTime? AnsweredAt,
    bool EmailSent,
    DateTime? EmailSentAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// الأسئلة اللي المستخدم نفسه أرسلها.
/// </summary>
public sealed record MySupportQuestionResponse(
    Guid Id,
    string Question,
    string? Answer,
    string Category,
    bool IsPublished,
    bool IsAnswered,
    DateTime? AnsweredAt,
    DateTime CreatedAt);
