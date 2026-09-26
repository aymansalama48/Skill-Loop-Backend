using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;

// تعريف كلاس الكيان يدوياً عشان ميتعارضش مع اسم الـ namespace
using BookingEntity = Skill_Loop.Domain.Entities.Booking.Booking;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandHandler(
    IApplicationDbContext _dbContext,
    ICurrentUser _currentUser) : ICommandHandler<CreateBookingCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) return Result<Guid>.Failure(UserErrors.Unauthorized);
        var learnerId = _currentUser.UserId.Value;

        // 1. جلب الجلسة للتأكد من سعرها ومدربها
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
            return Result<Guid>.Failure(new Error("Booking.SessionNotFound", "الجلسة غير موجودة.", ErrorType.NotFound));

        // 2. جلب محفظة الطالب
        var wallet = await _dbContext.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == learnerId, cancellationToken);

        if (wallet is null || wallet.Balance < session.PriceInCredits)
            return Result<Guid>.Failure(new Error("Booking.InsufficientFunds", "الرصيد غير كافٍ لإتمام الحجز.", ErrorType.Conflict));

        // 3. إنشاء كيان الحجز
        var bookingResult = BookingEntity.Create(
            session.Id,
            learnerId,
            session.InstructorId,
            request.ScheduleDate,
            request.StartTime,
            session.DurationInMinutes,
            session.PriceInCredits,
            session.Type);

        if (bookingResult.IsFailure) return Result<Guid>.Failure(bookingResult.Errors);
        var booking = bookingResult.Data;

        // 4. خصم الرصيد من الطالب وتسجيل العملية (Receipt)
        var deductResult = wallet.DeductCredits(session.PriceInCredits, booking.Id, $"حجز جلسة: {session.Title}");
        if (deductResult.IsFailure) return Result<Guid>.Failure(deductResult.Errors);

        // 5. حفظ كل التغييرات في قاعدة البيانات
        _dbContext.Add(booking);
        // ملاحظة: الـ EF Core بيعمل Track لتحديثات المحفظة لوحده لأننا جبناها بـ Tracking
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(booking.Id);
    }
}