using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Support.Share;

namespace Skill_Loop.Application.Features.Support.Queries.GetPublishedSupportQuestionsPaged;

/// <summary>
/// الـ FAQ العام. بيرجّع الأسئلة المنشورة فقط وبشكل مبسّط
/// من غير بريد المستخدم ولا اسمه.
/// </summary>
public sealed record GetPublishedSupportQuestionsPagedQuery(
    PaginationParameters Pagination,
    string? SearchTerm = null,
    string? Category = null) : ICacheableQuery<PagedResult<SupportQuestionResponse>>
{
    public string CacheKey =>
        $"support:questions:page:{Pagination?.PageNumber}:size:{Pagination?.PageSize}:search:{SearchTerm ?? ""}:category:{Category ?? ""}";

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}
