namespace Skill_Loop.Api.Contracts.Instructors;

public sealed record AddAvailabilityRequest(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime
);