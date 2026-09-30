using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;

public sealed class CancelBookingCommandHandler(
    IApplicationDbContext _dbContext,
    IDateTime _dateTime) : ICommandHandler<CancelBookingCommand>
{
    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        // UtcNow, not Now: Cancel(reason, utcNow) persists this as CancelledAtUtc, and it is
        // compared against ScheduledAtUtc. Using display-local time wrote a two-hour skew
        // onto every cancelled booking.
        var utcNow = _dateTime.UtcNow;

        var booking = await _dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound);
        }

        // 1. لازم نجيب الجلسة عشان نعرف مين المحاضر
        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == booking.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(BookingErrors.SessionNotFound);
        }

        // 2. الإلغاء مسموح للمتعلم نفسه أو للمحاضر
        var isLearner = booking.LearnerUserId == request.RequestedByUserId;
        var isInstructor = session.InstructorId == request.RequestedByUserId || session.OwnerId == request.RequestedByUserId;

        if (!isLearner && !isInstructor)
        {
            return Result.Failure(BookingErrors.NotLearner);
        }

        if (!booking.CanBeCancelled)
        {
            return Result.Failure(BookingErrors.CannotCancel);
        }

        // 3. تسجيل سبب الإلغاء (نفس الدالة بتعمل Emitting للحدث)
        var shouldRefund = booking.IsRefundable && booking.PriceInCredits > 0;
        var cancelResult = booking.Cancel(request.Reason, utcNow);

        if (cancelResult.IsFailure)
        {
            return Result.Failure(cancelResult.Errors.First());
        }

        // 4. استرجاع الـ Credits للمتعلم لو الحجز كان قابل للاسترجاع
        if (shouldRefund)
        {
            var refundResult = await BookingWalletHelper.RefundAsync(
                _dbContext, booking, session.Title, cancellationToken);

            if (refundResult.IsFailure)
            {
                return Result.Failure(refundResult.Errors.First());
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}