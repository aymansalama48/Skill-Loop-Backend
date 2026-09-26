using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Bookings.Commands.CompleteBooking;

public sealed class CompleteBookingCommandHandler(
    IApplicationDbContext _dbContext,
    ICurrentUser _currentUser) : ICommandHandler<CompleteBookingCommand, bool>
{
    public async Task<Result<bool>> Handle(CompleteBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) return Result<bool>.Failure(new Error("User.Unauthorized", "غير مصرح.", ErrorType.Unauthorized));

        // 1. جلب الحجز
        var booking = await _dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking is null) return Result<bool>.Failure(BookingErrors.NotFound);

        // 2. جلب الجلسة عشان نتحقق إن المدرب هو اللي يكمل
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == booking.SessionId, cancellationToken);

        if (session is null) return Result<bool>.Failure(BookingErrors.SessionNotFound);

        // 3. التأكد إن المدرب هو اللي بيقفل الجلسة
        if (session.InstructorId != _currentUser.UserId.Value)
            return Result<bool>.Failure(BookingErrors.NotSessionInstructor);

        // 4. إنهاء الحجز — بيحوّل الحالة لـ Completed ويرفع
        //    SessionCompletedDomainEvent اللي بيزود محفظة المدرب وعدد جلساته
        var completeResult = booking.Complete(session.InstructorId);

        if (completeResult.IsFailure)
            return Result<bool>.Failure(completeResult.Errors.First());

        // 5. تحديث حالة الجلسة الأصلية لـ Completed
        session.Complete();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
