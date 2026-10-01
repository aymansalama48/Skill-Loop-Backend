namespace Skill_Loop.Api.Contracts.Bookings;

public sealed record CreateBookingRequest(Guid SessionId);

public sealed record CancelBookingRequest(string? Reason = null);

public sealed record ChangeBookingStatusRequest(string? Reason = null);

public sealed record CreateDirectBookingRequest(Guid InstructorId, DateTime ScheduledAtUtc, int DurationMinutes);
