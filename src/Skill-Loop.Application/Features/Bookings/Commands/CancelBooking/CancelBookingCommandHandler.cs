using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;

public sealed class CancelBookingCommandHandler(
    IApplicationDbContext _dbContext,
    ICurrentUser _currentUser) : ICommandHandler<CancelBookingCommand, bool>
{
    public async Task<Result<bool>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<bool>.Failure(new Error("User.Unauthorized", "غير مصرح.", ErrorType.Unauthorized));

        var userId = _currentUser.UserId.Value;

        // 1. جلب الحجز
        var booking = await _dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking is null)
            return Result<bool>.Failure(new Error("Booking.NotFound", "الحجز غير موجود.", ErrorType.NotFound));

        // 2. التحقق من الصلاحيات (فقط الطالب صاحب الحجز أو المدرب يحق لهم الإلغاء)
        if (booking.LearnerUserId != userId && booking.InstructorId != userId)
            return Result<bool>.Failure(new Error("Booking.Forbidden", "لا تملك صلاحية إلغاء هذا الحجز.", ErrorType.Forbidden));

        // 3. إلغاء الحجز من الدومين
        var cancelResult = booking.Cancel();
        if (cancelResult.IsFailure) return Result<bool>.Failure(cancelResult.Errors);

        // 4. استرجاع الأموال لمحفظة الطالب (Refund)
        var studentWallet = await _dbContext.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == booking.LearnerUserId, cancellationToken);

        if (studentWallet is not null)
        {
            // استخدمنا دالة AddCredits اللي إنت عاملها في UserWallet
            var refundResult = studentWallet.AddCredits(
                booking.PricePaid,
                booking.Id,
                "استرداد رصيد لإلغاء الحجز");

            if (refundResult.IsFailure) return Result<bool>.Failure(refundResult.Errors);
        }

        // 5. حفظ التغييرات (الحجز اتلغى، والفلوس رجعت)
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}