using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Application.Features.Bookings.Shared;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Bookings.Queries.GetBookingById;

public sealed record GetBookingByIdQuery(Guid BookingId) : IQuery<BookingResponse>;

public sealed class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, BookingResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBookingByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<BookingResponse>> Handle(
        GetBookingByIdQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await _dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking is null)
        {
            return Result<BookingResponse>.Failure(BookingErrors.NotFound);
        }

        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == booking.SessionId, cancellationToken);

        if (session is null)
        {
            return Result<BookingResponse>.Failure(BookingErrors.SessionNotFound);
        }

        return Result<BookingResponse>.Success(BookingResponseFactory.Create(booking, session));
    }
}
