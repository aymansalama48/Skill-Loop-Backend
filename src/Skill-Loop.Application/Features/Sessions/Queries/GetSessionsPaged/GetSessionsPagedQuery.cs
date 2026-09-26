using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Queries.Share;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;

public sealed record GetSessionsPagedQuery(
    PaginationParameters Pagination,
    Guid? InstructorId = null,
    SessionStatus? Status = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    bool BookableOnly = false,
    int? MaxCredits = null) : ICacheableQuery<PagedResult<SessionResponse>>
{
    // يتغير مفتاح الكاش بناءً على الفلاتر
    public string CacheKey =>
        $"sessions:all:page:{Pagination?.PageNumber}:size:{Pagination?.PageSize}:instructor:{InstructorId}:status:{Status}:from:{FromUtc:O}:to:{ToUtc:O}:bookable:{BookableOnly}:maxcredits:{MaxCredits}";

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}
