using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Bookings.Shared;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Bookings.Queries.GetMyBookings;

public sealed class GetMyBookingsQueryHandler : IQueryHandler<GetMyBookingsQuery, PagedResult<BookingResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMyBookingsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<BookingResponse>>> Handle(
        GetMyBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var query = _dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.LearnerUserId == request.LearnerUserId);

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(b => b.Status == status);
        }

        if (request.UpcomingOnly)
        {
            query = query.Where(b => b.ScheduledAtUtc != null && b.ScheduledAtUtc >= utcNow);
        }
        else if (request.PastOnly)
        {
            query = query.Where(b => b.ScheduledAtUtc != null && b.ScheduledAtUtc < utcNow);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // الحجوزات الجاية الأول (لو فلترنا upcoming) وإلا الأحدث الأول
        var ordered = request.UpcomingOnly
            ? query.OrderBy(b => b.ScheduledAtUtc)
            : query.OrderByDescending(b => b.ScheduledAtUtc);

        var pagedBookings = await ordered
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = await BookingResponseFactory.CreateListAsync(_dbContext, pagedBookings, cancellationToken);

        return Result<PagedResult<BookingResponse>>.Success(new PagedResult<BookingResponse>
        {
            Items = items,
            Pagination = new PaginationMetadata
            {
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        });
    }
}
