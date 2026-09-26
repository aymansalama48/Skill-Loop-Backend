using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandHandler(
    IApplicationDbContext _dbContext) : ICommandHandler<CreateBookingCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // كل المواعيد متخزنة UTC، فبنقارن بـ UtcNow مش بـ IDateTime (بتوقيت مصر)
        var utcNow = DateTime.UtcNow;

        // 1. جلب الجلسة
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result<Guid>.Failure(BookingErrors.SessionNotFound);
        }

        // 2. يمنع المتعلم إنه يحجز جلسته هو
        if (session.InstructorId == request.LearnerUserId || session.OwnerId == request.LearnerUserId)
        {
            return Result<Guid>.Failure(BookingErrors.SelfBooking);
        }

        // 3. الجلسة لازم تكون منشورة ومعلولة ولسه في المستقبل
        if (session.Status != SessionStatus.Published)
        {
            return Result<Guid>.Failure(BookingErrors.SessionNotPublished);
        }

        if (session.ScheduledAtUtc is null)
        {
            return Result<Guid>.Failure(BookingErrors.SessionNotScheduled);
        }

        if (session.ScheduledAtUtc.Value <= utcNow)
        {
            return Result<Guid>.Failure(BookingErrors.SessionInThePast);
        }

        // 4. منع الحجز المكرر (Idempotency)
        var alreadyBooked = await _dbContext.Bookings
            .AnyAsync(b =>
                b.SessionId == request.SessionId &&
                b.LearnerUserId == request.LearnerUserId &&
                (b.Status == BookingStatus.Pending ||
                 b.Status == BookingStatus.Confirmed ||
                 b.Status == BookingStatus.InProgress ||
                 b.Status == BookingStatus.Completed),
                cancellationToken);

        if (alreadyBooked)
        {
            return Result<Guid>.Failure(BookingErrors.AlreadyBooked);
        }

        // 5. التأكد إن فيه مقاعد فاضية
        var activeBookingsCount = await _dbContext.Bookings
            .CountAsync(b =>
                b.SessionId == request.SessionId &&
                (b.Status == BookingStatus.Pending ||
                 b.Status == BookingStatus.Confirmed ||
                 b.Status == BookingStatus.InProgress ||
                 b.Status == BookingStatus.Completed),
                cancellationToken);

        if (!session.IsBookable(utcNow, activeBookingsCount))
        {
            return Result<Guid>.Failure(BookingErrors.SessionFull);
        }

        // 6. خصم الـ Credits من محفظة المتعلم بشكل ذري
        if (session.CreditsPrice > 0)
        {
            var wallet = await _dbContext.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == request.LearnerUserId, cancellationToken);

            if (wallet is null)
            {
                return Result<Guid>.Failure(BookingErrors.WalletNotFound);
            }

            var deductionResult = wallet.DeductCredits(
                session.CreditsPrice,
                session.Id,
                $"Booking live session: {session.Title}");

            if (deductionResult.IsFailure)
            {
                return Result<Guid>.Failure(deductionResult.Errors.First());
            }
        }

        // 7. إنشاء الحجز — المصنع بيرفع BookingCreatedDomainEvent تلقائياً
        var bookingResult = Booking.Create(
            session.Id,
            request.LearnerUserId,
            session.CreditsPrice,
            session.ScheduledAtUtc,
            utcNow);

        if (bookingResult.IsFailure)
        {
            return Result<Guid>.Failure(bookingResult.Errors.First());
        }

        var booking = bookingResult.Data!;
        _dbContext.Add(booking);
        session.AddBooking(booking);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(booking.Id);
    }
}
