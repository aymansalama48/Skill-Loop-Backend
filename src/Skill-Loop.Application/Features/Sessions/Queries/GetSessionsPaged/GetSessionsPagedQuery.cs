using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Queries.Share;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;

public sealed record GetSessionsPagedQuery(
    PaginationParameters Pagination,
    Guid? InstructorId = null) : ICacheableQuery<PagedResult<SessionResponse>>
{
    // يتغير مفتاح الكاش بناءً على رقم الصفحة وحجمها ومعرف المحاضر
    public string CacheKey => $"sessions:all:page:{Pagination.PageNumber}:size:{Pagination.PageSize}:instructor:{InstructorId}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}