namespace Skill_Loop.UnitTests.Features.Bookings.Queries.GetMyBookings;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Bookings.Queries.GetMyBookings;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GetMyBookingsQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetMyBookingsQueryHandler _handler;

    public GetMyBookingsQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetMyBookingsQueryHandler(_dbContext);
    }

    private async Task<Session> SeedSessionAsync(DateTime? scheduledAtUtc = null, int credits = 20)
    {
        var instructorId = Guid.NewGuid();
        var session = Session.Create(
            instructorId, instructorId, "Live session",
            scheduledAtUtc: scheduledAtUtc ?? DateTime.UtcNow.AddDays(2),
            creditsPrice: credits,
            maxParticipants: 10);

        session.Publish();
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return session;
    }

    private async Task<Booking> SeedBookingAsync(
        Guid sessionId,
        Guid learnerId,
        int credits = 20,
        BookingStatus? status = null)
    {
        var session = await _dbContext.FirstOrDefaultAsync(
            _dbContext.Sessions.Where(s => s.Id == sessionId), CancellationToken.None);

        var booking = Booking.Create(sessionId, learnerId, credits, session!.ScheduledAtUtc).Data!;
        if (status == BookingStatus.Cancelled)
        {
            booking.Cancel("test");
        }

        _dbContext.Add(booking);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return booking;
    }

    [Fact]
    public async Task Handle_WhenNoBookings_ReturnsEmptyPagedResult()
    {
        var query = new GetMyBookingsQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().BeEmpty();
        result.Data.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyBookingsOfTheGivenLearner()
    {
        var session = await SeedSessionAsync();
        var learnerId = Guid.NewGuid();
        var otherLearnerId = Guid.NewGuid();

        await SeedBookingAsync(session.Id, learnerId);
        await SeedBookingAsync(session.Id, otherLearnerId);

        var result = await _handler.Handle(new GetMyBookingsQuery(learnerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle();
        result.Data.Items[0].LearnerUserId.Should().Be(learnerId);
        result.Data.Items[0].SessionTitle.Should().Be("Live session");
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsMatchingBookingsOnly()
    {
        var session = await SeedSessionAsync();
        var learnerId = Guid.NewGuid();

        await SeedBookingAsync(session.Id, learnerId);
        await SeedBookingAsync(session.Id, learnerId, status: BookingStatus.Cancelled);

        var result = await _handler.Handle(
            new GetMyBookingsQuery(learnerId, Status: BookingStatus.Cancelled), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle();
        result.Data.Items[0].Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WithUpcomingOnly_ExcludesPastBookings()
    {
        var futureSession = await SeedSessionAsync(DateTime.UtcNow.AddDays(3));
        var pastSession = await SeedSessionAsync(DateTime.UtcNow.AddDays(-3));
        var learnerId = Guid.NewGuid();

        await SeedBookingAsync(futureSession.Id, learnerId);
        await SeedBookingAsync(pastSession.Id, learnerId);

        var result = await _handler.Handle(new GetMyBookingsQuery(learnerId, UpcomingOnly: true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle();
        result.Data.Items[0].SessionId.Should().Be(futureSession.Id);
    }

    [Fact]
    public async Task Handle_WithPagination_AppliesSkipAndTake()
    {
        var session = await SeedSessionAsync();
        var learnerId = Guid.NewGuid();

        for (var i = 0; i < 5; i++)
        {
            await SeedBookingAsync(session.Id, learnerId);
        }

        var result = await _handler.Handle(
            new GetMyBookingsQuery(learnerId, PageNumber: 2, PageSize: 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Pagination.TotalCount.Should().Be(5);
        result.Data.Pagination.CurrentPage.Should().Be(2);
        result.Data.Pagination.PageSize.Should().Be(2);
    }
}
