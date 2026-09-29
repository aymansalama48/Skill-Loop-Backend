using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Queries.Share;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetMySessionsPaged;

public sealed class GetMySessionsPagedQueryHandler : IQueryHandler<GetMySessionsPagedQuery, PagedResult<SessionResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMySessionsPagedQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<SessionResponse>>> Handle(
        GetMySessionsPagedQuery request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var query = _dbContext.Sessions
            .AsNoTracking()
            .Where(s => s.InstructorId == request.InstructorUserId || s.OwnerId == request.InstructorUserId);

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(s => s.Status == status);
        }

        if (request.UpcomingOnly)
        {
            query = query.Where(s => s.ScheduledAtUtc != null && s.ScheduledAtUtc >= utcNow);
        }
        else if (request.PastOnly)
        {
            query = query.Where(s => s.ScheduledAtUtc != null && s.ScheduledAtUtc < utcNow);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var ordered = request.UpcomingOnly
            ? query.OrderBy(s => s.ScheduledAtUtc)
            : query.OrderByDescending(s => s.ScheduledAtUtc ?? s.CreatedAt);

        var sessions = await ordered
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var sessionIds = sessions.Select(s => s.Id).ToList();

        var bookingCounts = await _dbContext.Bookings
            .AsNoTracking()
            .Where(b =>
                sessionIds.Contains(b.SessionId) &&
                (b.Status == BookingStatus.Pending ||
                 b.Status == BookingStatus.Confirmed ||
                 b.Status == BookingStatus.InProgress ||
                 b.Status == BookingStatus.Completed))
            .GroupBy(b => b.SessionId)
            .Select(g => new { SessionId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var countsBySession = bookingCounts.ToDictionary(x => x.SessionId, x => x.Count);

        var responses = sessions
            .Select(s => SessionAvailabilityHelper.BuildResponse(
                s,
                countsBySession.TryGetValue(s.Id, out var count) ? count : 0,
                utcNow))
            .ToList();

        return Result<PagedResult<SessionResponse>>.Success(new PagedResult<SessionResponse>
        {
            Items = responses,
            Pagination = new PaginationMetadata
            {
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        });
    }
}
