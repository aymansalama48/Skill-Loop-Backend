using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Queries.Share;

public sealed record SessionResponse(
    Guid Id,
    Guid InstructorId,
    Guid OwnerId,
    string InstructorName,
    string? InstructorAvatarUrl,
    double AverageRating,
    int TotalReviews,
    string Title,
    string? Description,
    SessionStatus Status,
    DateTime? ScheduledAtUtc,
    DateTime? EndsAtUtc,
    int DurationMinutes,
    int CreditsPrice,
    SessionLocationType LocationType,
    string? LocationDetails,
    int MaxParticipants,
    int BookedParticipants,
    int AvailableSlots,
    bool IsBookable,
    DateTime CreatedAt);
