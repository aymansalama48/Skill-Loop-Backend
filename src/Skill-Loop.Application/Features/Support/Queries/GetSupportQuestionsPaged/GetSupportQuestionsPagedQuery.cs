using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionsPaged;

/// <summary>
/// قائمة الاستفسارات الكاملة — لفريق الدعم والـ Admin بس.
/// الـ Endpoint العام بيستخدم <see cref="GetPublishedSupportQuestionsPagedQuery"/> اللي بيرجّع المنشور فقط.
/// </summary>
public sealed record GetSupportQuestionsPagedQuery(
    PaginationParameters Pagination,
    string? SearchTerm = null,
    string? Category = null,
    bool? IsPublished = null,
    bool? IsAnswered = null) : ICacheableQuery<PagedResult<SupportQuestionDetailsResponse>>
{
    public string CacheKey =>
        $"support:questions:manage:page:{Pagination?.PageNumber}:size:{Pagination?.PageSize}:search:{SearchTerm ?? ""}:category:{Category ?? ""}:published:{IsPublished}:answered:{IsAnswered}";

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}
