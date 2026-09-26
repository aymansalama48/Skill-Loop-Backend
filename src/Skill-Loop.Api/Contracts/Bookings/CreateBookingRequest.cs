namespace Skill_Loop.Api.Contracts.Bookings;

public sealed record CreateBookingRequest(
    Guid SessionId,
    DateTime ScheduleDate,
    TimeSpan StartTime
);