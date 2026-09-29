using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Bookings.Shared;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Bookings.Queries.GetSessionBookings;

public sealed class GetSessionBookingsQueryHandler
    : IQueryHandler<GetSessionBookingsQuery, PagedResult<BookingResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSessionBookingsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<BookingResponse>>> Handle(
        GetSessionBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var sessionExists = await _dbContext.Sessions
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.SessionId, cancellationToken);

        if (!sessionExists)
        {
            return Result<PagedResult<BookingResponse>>.Failure(BookingErrors.SessionNotFound);
        }

        var query = _dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.SessionId == request.SessionId);

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(b => b.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pagedBookings = await query
            .OrderByDescending(b => b.BookedAtUtc)
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
