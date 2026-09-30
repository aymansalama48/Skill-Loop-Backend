using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Application.Features.Bookings.Shared;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Bookings.Queries.GetBookingById;

public sealed class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, BookingResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetBookingByIdQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
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

        //Ownership: only the learner who booked, the session owner, or a global booking manager.
        var currentUserId = _currentUser.UserId;
        var isLearner = currentUserId.HasValue && booking.LearnerUserId == currentUserId.Value;
        var isInstructor = currentUserId.HasValue &&
                           (session.InstructorId == currentUserId.Value || session.OwnerId == currentUserId.Value);
        var canManageAll = _currentUser.HasPermission(Permissions.Bookings.ViewAll)
                           || _currentUser.HasPermission(Permissions.Bookings.ManageAll);

        if (!isLearner && !isInstructor && !canManageAll)
        {
            return Result<BookingResponse>.Failure(BookingErrors.NotLearner);
        }

        return Result<BookingResponse>.Success(BookingResponseFactory.Create(booking, session));
    }
}
