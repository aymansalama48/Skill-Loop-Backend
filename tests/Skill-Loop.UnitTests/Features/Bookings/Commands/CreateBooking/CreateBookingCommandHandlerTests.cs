namespace Skill_Loop.UnitTests.Features.Bookings.Commands.CreateBooking;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Entities.Wallets;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CreateBookingCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly CreateBookingCommandHandler _handler;

    public CreateBookingCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new CreateBookingCommandHandler(_dbContext);
    }

    private async Task<Session> SeedPublishedSessionAsync(
        int creditsPrice = 20,
        int maxParticipants = 3,
        DateTime? scheduledAtUtc = null)
    {
        var instructorId = Guid.NewGuid();
        var session = Session.Create(
            instructorId,
            instructorId,
            "Live C# Session",
            "Full description",
            scheduledAtUtc ?? DateTime.UtcNow.AddDays(2),
            60,
            creditsPrice,
            SessionLocationType.Online,
            "https://meet.example.com/abc",
            maxParticipants);

        session.Publish();
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return session;
    }

    private async Task SeedWalletAsync(Guid userId, int balance)
    {
        _dbContext.Add(UserWallet.Create(userId, balance));
        await _dbContext.SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenSessionDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new CreateBookingCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_SESSION_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WhenSessionIsNotPublished_ReturnsConflictFailure()
    {
        var session = Session.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Draft session", scheduledAtUtc: DateTime.UtcNow.AddDays(2));
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateBookingCommand(session.Id, Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_SESSION_NOT_PUBLISHED");
    }

    [Fact]
    public async Task Handle_WhenSessionIsInThePast_ReturnsConflictFailure()
    {
        var session = await SeedPublishedSessionAsync(scheduledAtUtc: DateTime.UtcNow.AddHours(-2));

        var command = new CreateBookingCommand(session.Id, Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_SESSION_IN_THE_PAST");
    }

    [Fact]
    public async Task Handle_WhenLearnerBooksOwnSession_ReturnsValidationFailure()
    {
        var session = await SeedPublishedSessionAsync();
        var instructorId = session.InstructorId;

        var command = new CreateBookingCommand(session.Id, instructorId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_SELF_BOOKING");
    }

    [Fact]
    public async Task Handle_ValidBooking_CreatesConfirmedBooking()
    {
        var session = await SeedPublishedSessionAsync(creditsPrice: 0);
        var learnerId = Guid.NewGuid();

        var command = new CreateBookingCommand(session.Id, learnerId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var booking = await _dbContext.FirstOrDefaultAsync(_dbContext.Bookings.Where(b => b.Id == result.Data));
        booking.Should().NotBeNull();
        booking!.SessionId.Should().Be(session.Id);
        booking.LearnerUserId.Should().Be(learnerId);
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task Handle_PaidSession_DeductsCreditsFromLearnerWallet()
    {
        var session = await SeedPublishedSessionAsync(creditsPrice: 30);
        var learnerId = Guid.NewGuid();
        await SeedWalletAsync(learnerId, 100);

        var command = new CreateBookingCommand(session.Id, learnerId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var wallet = await _dbContext.FirstOrDefaultAsync(_dbContext.UserWallets.Where(w => w.UserId == learnerId));
        wallet!.Balance.Should().Be(70);
    }

    [Fact]
    public async Task Handle_WhenWalletHasInsufficientCredits_ReturnsFailure()
    {
        var session = await SeedPublishedSessionAsync(creditsPrice: 500);
        var learnerId = Guid.NewGuid();
        await SeedWalletAsync(learnerId, 10);

        var command = new CreateBookingCommand(session.Id, learnerId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Wallet.InsufficientBalance");
    }

    [Fact]
    public async Task Handle_WhenWalletIsMissing_ReturnsNotFoundFailure()
    {
        var session = await SeedPublishedSessionAsync(creditsPrice: 50);

        var command = new CreateBookingCommand(session.Id, Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_WALLET_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WhenLearnerAlreadyBooked_ReturnsConflictFailure()
    {
        var session = await SeedPublishedSessionAsync(creditsPrice: 0);
        var learnerId = Guid.NewGuid();

        var first = await _handler.Handle(new CreateBookingCommand(session.Id, learnerId), CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var second = await _handler.Handle(new CreateBookingCommand(session.Id, learnerId), CancellationToken.None);

        second.IsFailure.Should().BeTrue();
        second.Errors.Should().Contain(e => e.Code == "BOOKING_ALREADY_BOOKED");
    }

    [Fact]
    public async Task Handle_WhenAllSlotsTaken_ReturnsSessionFullFailure()
    {
        var session = await SeedPublishedSessionAsync(creditsPrice: 0, maxParticipants: 1);

        var first = await _handler.Handle(new CreateBookingCommand(session.Id, Guid.NewGuid()), CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var second = await _handler.Handle(new CreateBookingCommand(session.Id, Guid.NewGuid()), CancellationToken.None);

        second.IsFailure.Should().BeTrue();
        second.Errors.Should().Contain(e => e.Code == "BOOKING_SESSION_FULL");
    }
}
