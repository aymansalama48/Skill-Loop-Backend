namespace Skill_Loop.UnitTests.Features.Bookings.Queries.GetSessionBookings;

using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Bookings.Queries.GetSessionBookings;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GetSessionBookingsQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUser;
    private readonly Mock<IUserManagementService> _userService;
    private readonly GetSessionBookingsQueryHandler _handler;

    // Read lazily by the Moq setup below so each test can switch the acting user.
    private Guid _actingUserId;

    public GetSessionBookingsQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _currentUser = new Mock<ICurrentUser>();
        _currentUser.Setup(c => c.UserId).Returns(() => _actingUserId);
        _currentUser.Setup(c => c.HasPermission(It.IsAny<string>())).Returns(false);
        _userService = new Mock<IUserManagementService>();
        _handler = new GetSessionBookingsQueryHandler(_dbContext, _currentUser.Object, _userService.Object);
    }

    private async Task<Session> SeedSessionAsync()
    {
        var instructorId = Guid.NewGuid();
        var session = Session.Create(
            instructorId, instructorId, "Group session",
            scheduledAtUtc: DateTime.UtcNow.AddDays(1),
            creditsPrice: 15,
            maxParticipants: 5);

        session.Publish();
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Default acting user is the session's instructor.
        _actingUserId = instructorId;

        return session;
    }

    [Fact]
    public async Task Handle_WhenSessionDoesNotExist_ReturnsNotFoundFailure()
    {
        var result = await _handler.Handle(new GetSessionBookingsQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_SESSION_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ReturnsAllBookingsOfTheSession()
    {
        var session = await SeedSessionAsync();

        _dbContext.Add(Booking.Create(session.Id, Guid.NewGuid(), 15).Data!);
        _dbContext.Add(Booking.Create(session.Id, Guid.NewGuid(), 15).Data!);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetSessionBookingsQuery(session.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Should().OnlyContain(b => b.SessionId == session.Id);
        result.Data.Items[0].SessionTitle.Should().Be("Group session");
    }

    [Fact]
    public async Task Handle_WithStatusFilter_FiltersBookings()
    {
        var session = await SeedSessionAsync();

        _dbContext.Add(Booking.Create(session.Id, Guid.NewGuid(), 15).Data!);
        var cancelled = Booking.Create(session.Id, Guid.NewGuid(), 15).Data!;
        cancelled.Cancel("no longer needed");
        _dbContext.Add(cancelled);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(
            new GetSessionBookingsQuery(session.Id, Status: BookingStatus.Cancelled), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle();
        result.Data.Items[0].Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotInstructorOrManager_ReturnsNotLearnerFailure()
    {
        var session = await SeedSessionAsync();
        _dbContext.Add(Booking.Create(session.Id, Guid.NewGuid(), 15).Data!);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _actingUserId = Guid.NewGuid();

        var result = await _handler.Handle(new GetSessionBookingsQuery(session.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "BOOKING_NOT_LEARNER");
    }

    [Fact]
    public async Task Handle_WhenCallerHasManageAllPermission_ReturnsBookings()
    {
        var session = await SeedSessionAsync();
        _dbContext.Add(Booking.Create(session.Id, Guid.NewGuid(), 15).Data!);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _actingUserId = Guid.NewGuid();
        _currentUser.Setup(c => c.HasPermission("Bookings.ManageAll")).Returns(true);

        var result = await _handler.Handle(new GetSessionBookingsQuery(session.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle();
    }
}
