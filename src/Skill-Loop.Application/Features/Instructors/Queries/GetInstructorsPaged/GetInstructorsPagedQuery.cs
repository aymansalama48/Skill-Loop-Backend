using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Instructors.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorsPaged;

[AuthenticatedOnly]
public sealed record GetInstructorsPagedQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    double? MinRating,
    bool? HasCompletedSessions,
    string? SortBy
) : ICacheableQuery<PagedResult<InstructorSummaryResponse>> 
{
    // دمجنا كل الفلاتر في الـ CacheKey عشان لو الفلتر اتغير، يجيب كاش مختلف
    public string CacheKey =>
        $"instructors-list-approved-{PageNumber}-{PageSize}-{SearchTerm ?? "none"}-{MinRating ?? 0}-{HasCompletedSessions ?? false}-{SortBy ?? "default"}";

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(5);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(30);
}