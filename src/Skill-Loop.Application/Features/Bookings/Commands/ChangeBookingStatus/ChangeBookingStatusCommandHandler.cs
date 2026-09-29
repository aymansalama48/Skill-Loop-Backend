using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Bookings.Commands.ChangeBookingStatus;

public sealed class ChangeBookingStatusCommandHandler(
    IApplicationDbContext _dbContext,
    IDateTime _dateTime) : ICommandHandler<ChangeBookingStatusCommand>
{
    public async Task<Result> Handle(ChangeBookingStatusCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _dateTime.Now;

        var booking = await _dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound);
        }

        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == booking.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(BookingErrors.SessionNotFound);
        }

        // 1. صلاحية المحاضر فقط
        if (session.InstructorId != request.InstructorUserId && session.OwnerId != request.InstructorUserId)
        {
            return Result.Failure(BookingErrors.NotSessionInstructor);
        }

        // 2. تنفيذ التحويل على مستوى الـ Domain
        Result transitionResult = request.NewStatus switch
        {
            BookingStatus.Confirmed => booking.Confirm(),
            BookingStatus.InProgress => booking.Start(utcNow),
            BookingStatus.Completed => booking.Complete(session.InstructorId, utcNow),
            BookingStatus.Rejected => booking.Reject(request.Reason, utcNow),
            BookingStatus.NoShow => booking.MarkNoShow(utcNow),
            _ => Result.Failure(BookingErrors.InvalidStatusChange)
        };

        if (transitionResult.IsFailure)
        {
            return Result.Failure(transitionResult.Errors.First());
        }

        // 3. لو اترفض، الـ credits ترجع للمتعلم
        if (request.NewStatus == BookingStatus.Rejected)
        {
            var refundResult = await BookingWalletHelper.RefundAsync(
                _dbContext, booking, session.Title, cancellationToken);

            if (refundResult.IsFailure)
            {
                return Result.Failure(refundResult.Errors.First());
            }
        }

        // 4. لو خلصت كل الحجوزات النشطة لل الجلسة، نعلّمها Completed
        if (request.NewStatus == BookingStatus.Completed)
        {
            var remainingActive = await _dbContext.Bookings
                .CountAsync(b =>
                    b.SessionId == session.Id &&
                    b.Id != booking.Id &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress),
                    cancellationToken);

            if (remainingActive == 0)
            {
                session.Complete();
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}