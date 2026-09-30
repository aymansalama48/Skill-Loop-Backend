using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Queries.Share;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetMySessionsPaged;

/// <summary>
/// لوحة المحاضر: جلساتي (upcoming / past / by-status) — الجزء الأول من الـ Teach dashboard
/// </summary>
[AuthenticatedOnly]
public sealed record GetMySessionsPagedQuery(
    Guid InstructorUserId,
    int PageNumber = 1,
    int PageSize = 10,
    SessionStatus? Status = null,
    bool UpcomingOnly = false,
    bool PastOnly = false) : ICacheableQuery<PagedResult<SessionResponse>>
{
    public string CacheKey =>
        $"sessions:mine:{InstructorUserId}:page:{PageNumber}:size:{PageSize}:status:{Status}:upcoming:{UpcomingOnly}:past:{PastOnly}";

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(5);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(30);
}
