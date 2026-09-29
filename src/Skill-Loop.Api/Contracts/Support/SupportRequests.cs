using Skill_Loop.Api.Contracts.Common;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Share;

namespace Skill_Loop.Api.Contracts.Support;

// ==============================
//Requests العامة (Endpoint anonymously)
// ==============================

/// <summary>
/// البحث في الـ FAQ العام. مش بيقبل أي فلتر على حالة النشر —
/// الـ Endpoint بيرجّع المنشور بس ومن غير أي بيانات شخصية.
/// </summary>
public sealed record GetPublishedSupportQuestionsRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Category = null);

// ==============================
// Requests المستخدم المسجّل
// ==============================

/// <summary>
/// استفسار جديد. الاسم والبريد بيتقرؤا من الـ JWT مش من الـ Body.
/// </summary>
public sealed record SubmitContactFormRequest(
    string Subject,
    string Message,
    string Category = "General");

public sealed record GetMySupportQuestionsRequest(
    int PageNumber = 1,
    int PageSize = 10);

// ==============================
// Requests فريق الدعم (Admin/Staff)
// ==============================

public sealed record GetSupportQuestionsRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Category = null,
    bool? IsPublished = null,
    bool? IsAnswered = null);

public sealed record CreateSupportQuestionRequest(
    string Question,
    string? Answer,
    string Category,
    bool IsPublished = false);

public sealed record UpdateSupportQuestionRequest(
    string Question,
    string? Answer,
    string Category,
    bool IsPublished);

public sealed record AnswerSupportQuestionRequest(
    string Answer,
    bool Publish = false);

public sealed record SetSupportQuestionPublicationRequest(
    bool IsPublished);

// ==============================
// Responses
// ==============================

public sealed record GetPublishedSupportQuestionsResponse(
    IReadOnlyList<SupportQuestionResponse> Items,
    PaginationMetadata Pagination);

public sealed record GetMySupportQuestionsResponse(
    IReadOnlyList<MySupportQuestionResponse> Items,
    PaginationMetadata Pagination);

public sealed record GetSupportQuestionsResponse(
    IReadOnlyList<SupportQuestionDetailsResponse> Items,
    PaginationMetadata Pagination);
