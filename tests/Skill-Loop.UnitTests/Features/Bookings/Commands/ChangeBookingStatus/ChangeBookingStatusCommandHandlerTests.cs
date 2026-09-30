namespace Skill_Loop.UnitTests.Features.Bookings.Commands.ChangeBookingStatus;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Bookings.Commands.ChangeBookingStatus;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Entities.Sessions.Events;
using Skill_Loop.Domain.Entities.Wallets;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Infrastructure.Core;
using Skill_Loop.UnitTests.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ChangeBookingStatusCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ChangeBookingStatusCommandHandler _handler;
    private readonly IDateTime _dateTime;

    public ChangeBookingStatusCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _dateTime = new DateTimeProvider();
        _handler = new ChangeBookingStatusCommandHandler(_dbContext, _dateTime);
    }

    private async Task<(Session Session, Booking Booking, Guid LearnerId, Guid InstructorId)> SeedAsync(int credits = 30)
    {
        var instructorId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();

        var session = Session.Create(
            instructorId, instructorId, "Live session",
            scheduledAtUtc: DateTime.UtcNow.AddDays(1),
            creditsPrice: credits,
            maxParticipants: 5);

        session.Publish();
        _dbContext.Add(session);

        // المحفظة بتبدأ بـ 100 وبعد خصم سعر الجلسة بتبقى 70
        var wallet = UserWallet.Create(learnerId, 100);
        wallet.DeductCredits(credits, session.Id, "Booking live session");
        _dbContext.Add(wallet);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var booking = Booking.Create(session.Id, learnerId, credits, session.ScheduledAtUtc).Data!;
        _dbContext.Add(booking);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return (session, booking, learnerId, instructorId);
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotInstructor_ReturnsForbidden()
    {
        var (_, booking, learnerId, _) = await SeedAsync();

        var command = new ChangeBookingStatusCommand(booking.Id, learnerId, BookingStatus.Completed);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_NOT_SESSION_INSTRUCTOR");
    }

    [Fact]
    public async Task Handle_ToInProgress_SetsBookingStatus()
    {
        var (_, booking, _, instructorId) = await SeedAsync();

        var command = new ChangeBookingStatusCommand(booking.Id, instructorId, BookingStatus.InProgress);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.FirstOrDefaultAsync(_dbContext.Bookings.Where(b => b.Id == booking.Id));
        persisted!.Status.Should().Be(BookingStatus.InProgress);
        persisted.StartedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ToCompleted_RaisesSessionCompletedEventAndCompletesSession()
    {
        var (session, booking, _, instructorId) = await SeedAsync();

        var command = new ChangeBookingStatusCommand(booking.Id, instructorId, BookingStatus.Completed);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.FirstOrDefaultAsync(_dbContext.Bookings.Where(b => b.Id == booking.Id));
        persisted!.Status.Should().Be(BookingStatus.Completed);
        persisted.CompletedAtUtc.Should().NotBeNull();
        persisted.DomainEvents.Should().ContainSingle(e => e is SessionCompletedDomainEvent);

        var persistedSession = await _dbContext.FirstOrDefaultAsync(_dbContext.Sessions.Where(s => s.Id == session.Id));
        persistedSession!.Status.Should().Be(SessionStatus.Completed);
    }

    [Fact]
    public async Task Handle_ToRejected_RefundsLearnerAndDoesNotCompleteSession()
    {
        var (session, booking, learnerId, instructorId) = await SeedAsync(credits: 30);

        var command = new ChangeBookingStatusCommand(booking.Id, instructorId, BookingStatus.Rejected, "Not available");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.FirstOrDefaultAsync(_dbContext.Bookings.Where(b => b.Id == booking.Id));
        persisted!.Status.Should().Be(BookingStatus.Rejected);

        var wallet = await _dbContext.FirstOrDefaultAsync(_dbContext.UserWallets.Where(w => w.UserId == learnerId));
        wallet!.Balance.Should().Be(100);

        var persistedSession = await _dbContext.FirstOrDefaultAsync(_dbContext.Sessions.Where(s => s.Id == session.Id));
        persistedSession!.Status.Should().Be(SessionStatus.Published);
    }

    [Fact]
    public async Task Handle_ToNoShow_SetsStatusWithoutRefund()
    {
        var (_, booking, learnerId, instructorId) = await SeedAsync(credits: 30);

        var command = new ChangeBookingStatusCommand(booking.Id, instructorId, BookingStatus.NoShow);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.FirstOrDefaultAsync(_dbContext.Bookings.Where(b => b.Id == booking.Id));
        persisted!.Status.Should().Be(BookingStatus.NoShow);

        var wallet = await _dbContext.FirstOrDefaultAsync(_dbContext.UserWallets.Where(w => w.UserId == learnerId));

        // المتعلم مش بيلغي — الرصيد بيفضل 70 (اللي اتخصم وقت الحجز)
        wallet!.Balance.Should().Be(70);
    }

    [Fact]
    public async Task Handle_WhenOtherBookingsStillActive_DoesNotCompleteSession()
    {
        var (session, booking, _, instructorId) = await SeedAsync(credits: 0);

        var otherBooking = Booking.Create(session.Id, Guid.NewGuid(), 0, session.ScheduledAtUtc).Data!;
        _dbContext.Add(otherBooking);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new ChangeBookingStatusCommand(booking.Id, instructorId, BookingStatus.Completed);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persistedSession = await _dbContext.FirstOrDefaultAsync(_dbContext.Sessions.Where(s => s.Id == session.Id));
        persistedSession!.Status.Should().Be(SessionStatus.Published);
    }
}
