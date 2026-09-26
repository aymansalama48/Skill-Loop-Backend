using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
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

        if (booking is null) return Result<bool>.Failure(new Error("Booking.NotFound", "الحجز غير موجود.", ErrorType.NotFound));

        // التأكد إن المدرب هو اللي بيقفل الجلسة (أو الطالب حسب البيزنس عندك)
        if (booking.InstructorId != _currentUser.UserId.Value)
            return Result<bool>.Failure(new Error("Booking.Unauthorized", "المدرب فقط يمكنه إنهاء الجلسة.", ErrorType.Unauthorized));

        // 2. جلب الجلسة الأصلية
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == booking.SessionId, cancellationToken);

        if (session is null) return Result<bool>.Failure(new Error("Session.NotFound", "الجلسة غير موجودة.", ErrorType.NotFound));

        // 3. إنهاء الحجز (بيغير حالته لـ Completed)
        booking.Complete();

        // 4. السحر هنا: بننادي دالة Complete اللي جوا الجلسة! 
        // الدالة دي بترمي SessionCompletedDomainEvent في الـ Outbox اللي هيسمع في المحفظة والبروفايل
        session.Complete(booking.LearnerUserId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}