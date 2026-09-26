using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Application.Features.Bookings.Commands.CompleteBooking;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Entities.Wallets;
using Skill_Loop.Domain.Entities.Sessions.Events;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Skill_Loop.UnitTests.Features.Bookings.Commands.CompleteBooking;

public class CompleteBookingCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUser;
    private readonly CompleteBookingCommandHandler _handler;

    public CompleteBookingCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _currentUser = new Mock<ICurrentUser>();
        _handler = new CompleteBookingCommandHandler(_dbContext, _currentUser.Object);
    }

    private async Task<(Session Session, Booking Booking, Guid InstructorId)> SeedAsync(int credits = 40)
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

        var wallet = UserWallet.Create(learnerId, 100);
        wallet.DeductCredits(credits, session.Id, "Booking live session");
        _dbContext.Add(wallet);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var booking = Booking.Create(session.Id, learnerId, credits, session.ScheduledAtUtc).Data!;
        _dbContext.Add(booking);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return (session, booking, instructorId);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorized()
    {
        _currentUser.Setup(c => c.UserId).Returns((Guid?)null);
        var command = new CompleteBookingCommand(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "User.Unauthorized");
    }

    [Fact]
    public async Task Handle_WhenBookingDoesNotExist_ReturnsNotFoundFailure()
    {
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());

        var command = new CompleteBookingCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == BookingErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNotInstructor_ReturnsForbidden()
    {
        var (_, booking, instructorId) = await SeedAsync();

        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());

        var command = new CompleteBookingCommand(booking.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == BookingErrors.NotSessionInstructor.Code);
    }

    [Fact]
    public async Task Handle_WhenBookingAlreadyCompleted_ReturnsConflictFailure()
    {
        var (session, booking, instructorId) = await SeedAsync();
        booking.Complete(instructorId);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _currentUser.Setup(c => c.UserId).Returns(instructorId);

        var command = new CompleteBookingCommand(booking.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public async Task Handle_ByValidInstructor_CompletesBookingAndSession()
    {
        var (session, booking, instructorId) = await SeedAsync();

        _currentUser.Setup(c => c.UserId).Returns(instructorId);

        var command = new CompleteBookingCommand(booking.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var persistedBooking = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Bookings.Where(b => b.Id == booking.Id), CancellationToken.None);
        persistedBooking!.Status.Should().Be(BookingStatus.Completed);
        persistedBooking.CompletedAtUtc.Should().NotBeNull();
        persistedBooking.DomainEvents.Should().ContainSingle(e => e is SessionCompletedDomainEvent);

        var persistedSession = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Sessions.Where(s => s.Id == session.Id), CancellationToken.None);
        persistedSession!.Status.Should().Be(SessionStatus.Completed);
    }
}
