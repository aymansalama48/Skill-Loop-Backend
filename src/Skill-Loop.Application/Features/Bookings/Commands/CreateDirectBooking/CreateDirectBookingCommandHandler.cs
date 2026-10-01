using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateDirectBooking;

public sealed class CreateDirectBookingCommandHandler(
    IApplicationDbContext _dbContext,
    IDateTime _dateTime) : ICommandHandler<CreateDirectBookingCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDirectBookingCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _dateTime.UtcNow;
        
        if (request.ScheduledAtUtc <= utcNow)
        {
            return Result<Guid>.Failure(BookingErrors.SessionInThePast);
        }

        // 1. إنشاء الجلسة في الكواليس (Bridging Gap)
        var session = Session.Create(
            instructorId: request.InstructorId,
            ownerId: request.InstructorId,
            title: "1-on-1 Session",
            description: "Auto-created session for direct booking",
            scheduledAtUtc: request.ScheduledAtUtc,
            durationMinutes: request.DurationMinutes,
            creditsPrice: 50, // السعر الافتراضي مثلا 50
            locationType: SessionLocationType.Online,
            locationDetails: null,
            maxParticipants: 1
        );
        
        session.Publish(); // الجلسة لازم تكون منشورة عشان يتم الحجز عليها
        _dbContext.Add(session);
        
        // 2. خصم الـ Credits من محفظة المتعلم
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

        // 3. إنشاء الحجز — المصنع بيرفع BookingCreatedDomainEvent تلقائياً
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
