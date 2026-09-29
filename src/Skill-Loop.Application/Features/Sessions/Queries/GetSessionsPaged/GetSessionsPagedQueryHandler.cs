using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Queries.Share;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;

public sealed class GetSessionsPagedQueryHandler : IQueryHandler<GetSessionsPagedQuery, PagedResult<SessionResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSessionsPagedQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<SessionResponse>>> Handle(GetSessionsPagedQuery request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var query = _dbContext.Sessions.AsNoTracking();

        if (request.InstructorId.HasValue)
        {
            query = query.Where(s => s.InstructorId == request.InstructorId.Value);
        }

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(s => s.Status == status);
        }

        if (request.FromUtc.HasValue)
        {
            var from = request.FromUtc.Value;
            query = query.Where(s => s.ScheduledAtUtc != null && s.ScheduledAtUtc >= from);
        }

        if (request.ToUtc.HasValue)
        {
            var to = request.ToUtc.Value;
            query = query.Where(s => s.ScheduledAtUtc != null && s.ScheduledAtUtc <= to);
        }

        if (request.MaxCredits.HasValue)
        {
            var maxCredits = request.MaxCredits.Value;
            query = query.Where(s => s.CreditsPrice <= maxCredits);
        }

        if (request.BookableOnly)
        {
            query = query.Where(s => s.Status == SessionStatus.Published && s.ScheduledAtUtc > utcNow);
        }

        query = query.OrderByDescending(s => s.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var sessions = await query
            .Skip(request.Pagination.Skip)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        // نقيس الحجوزات النشطة لكل الجلسات في صفحة واحدة (batch) بدل N+1
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

        var paginationMetadata = new PaginationMetadata
        {
            CurrentPage = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };

        var pagedResult = new PagedResult<SessionResponse>
        {
            Items = responses,
            Pagination = paginationMetadata
        };

        return Result<PagedResult<SessionResponse>>.Success(pagedResult);
    }
}
