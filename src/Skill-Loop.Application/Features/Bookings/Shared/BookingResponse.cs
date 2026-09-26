using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Bookings.Shared;

public sealed record BookingResponse(
    Guid Id,
    Guid SessionId,
    string SessionTitle,
    Guid InstructorId,
    Guid LearnerUserId,
    int PriceInCredits,
    BookingStatus Status,
    DateTime? ScheduledAtUtc,
    int DurationMinutes,
    SessionLocationType LocationType,
    string? LocationDetails,
    DateTime BookedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? CancelledAtUtc,
    string? CancellationReason);
