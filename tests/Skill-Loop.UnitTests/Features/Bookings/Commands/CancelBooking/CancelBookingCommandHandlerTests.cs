namespace Skill_Loop.UnitTests.Features.Bookings.Commands.CancelBooking;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Entities.Wallets;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Infrastructure.Core;
using Skill_Loop.UnitTests.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CancelBookingCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly CancelBookingCommandHandler _handler;
    private readonly IDateTime _dateTime;

    public CancelBookingCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new CancelBookingCommandHandler(_dbContext, _dateTime);
        _dateTime = new DateTimeProvider();
    }

    private async Task<(Session Session, Booking Booking, Guid LearnerId, Guid InstructorId)> SeedAsync(int credits = 40)
    {
        var instructorId = Guid.NewGuid();
        var learnerId = Guid.NewGuid();

        var session = Session.Create(
            instructorId, instructorId, "Live session",
            scheduledAtUtc: DateTime.UtcNow.AddDays(3),
            creditsPrice: credits,
            maxParticipants: 5);

        session.Publish();
        _dbContext.Add(session);

        // المحفظةstart بـ 100 وبعد خصم سعر الجلسة بتبقى 60
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
    public async Task Handle_WhenBookingDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new CancelBookingCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNeitherLearnerNorInstructor_ReturnsForbidden()
    {
        var (_, booking, _, _) = await SeedAsync();

        var command = new CancelBookingCommand(booking.Id, Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_NOT_LEARNER");
    }

    [Fact]
    public async Task Handle_CancelByLearner_SetsStatusToCancelledAndRefundsCredits()
    {
        var (_, booking, learnerId, _) = await SeedAsync(credits: 40);

        var command = new CancelBookingCommand(booking.Id, learnerId, "Cannot make it");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.FirstOrDefaultAsync(_dbContext.Bookings.Where(b => b.Id == booking.Id));
        persisted!.Status.Should().Be(BookingStatus.Cancelled);
        persisted.CancellationReason.Should().Be("Cannot make it");

        var wallet = await _dbContext.FirstOrDefaultAsync(_dbContext.UserWallets.Where(w => w.UserId == learnerId));
        wallet!.Balance.Should().Be(100);
    }

    [Fact]
    public async Task Handle_CancelByInstructor_RefundsLearner()
    {
        var (_, booking, learnerId, instructorId) = await SeedAsync(credits: 25);

        var command = new CancelBookingCommand(booking.Id, instructorId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var wallet = await _dbContext.FirstOrDefaultAsync(_dbContext.UserWallets.Where(w => w.UserId == learnerId));
        wallet!.Balance.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WhenBookingAlreadyCompleted_ReturnsConflictFailure()
    {
        var (_, booking, learnerId, instructorId) = await SeedAsync(credits: 0);
        booking.Complete(instructorId);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CancelBookingCommand(booking.Id, learnerId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_CANNOT_CANCEL");
    }
}
