namespace Skill_Loop.Application.Features.Instructors.Share;

public sealed record InstructorAvailabilityResponse(
    Guid Id,
    string DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime
);