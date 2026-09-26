using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Application.Features.Bookings.Queries.GetBookingById;
using Skill_Loop.Domain.Entities.Booking;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Skill_Loop.UnitTests.Features.Bookings.Queries.GetBookingById;

public class GetBookingByIdQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetBookingByIdQueryHandler _handler;

    public GetBookingByIdQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetBookingByIdQueryHandler(_dbContext);
    }

    private async Task<(Session Session, Booking Booking)> SeedAsync(int credits = 20)
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
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var booking = Booking.Create(session.Id, learnerId, credits, session.ScheduledAtUtc).Data!;
        _dbContext.Add(booking);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return (session, booking);
    }

    [Fact]
    public async Task Handle_WhenBookingDoesNotExist_ReturnsNotFoundFailure()
    {
        var query = new GetBookingByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == BookingErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenBookingExists_ReturnsBookingResponse()
    {
        var (session, booking) = await SeedAsync();

        var query = new GetBookingByIdQuery(booking.Id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(booking.Id);
        result.Data.SessionId.Should().Be(session.Id);
        result.Data.SessionTitle.Should().Be("Live session");
        result.Data.InstructorId.Should().Be(session.InstructorId);
        result.Data.Status.Should().Be(BookingStatus.Confirmed);
        result.Data.DurationMinutes.Should().Be(session.DurationMinutes);
        result.Data.LocationType.Should().Be(session.LocationType);
    }
}
