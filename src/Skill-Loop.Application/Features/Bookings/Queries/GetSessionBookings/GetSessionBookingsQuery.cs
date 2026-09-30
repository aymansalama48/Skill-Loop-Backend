using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Bookings.Shared;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Bookings.Queries.GetSessionBookings;

[AuthenticatedOnly]
public sealed record GetSessionBookingsQuery(
    Guid SessionId,
    int PageNumber = 1,
    int PageSize = 10,
    BookingStatus? Status = null) : ICacheableQuery<PagedResult<BookingResponse>>
{
    public string CacheKey =>
        $"bookings:session:{SessionId}:page:{PageNumber}:size:{PageSize}:status:{Status}";

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(5);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(30);
}
