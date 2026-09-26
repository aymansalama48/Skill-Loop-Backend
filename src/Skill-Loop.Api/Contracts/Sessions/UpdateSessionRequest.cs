using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Contracts.Sessions;

public sealed record UpdateSessionRequest(
    string Title,
    string? Description = null,
    DateTime? ScheduledAtUtc = null,
    int? DurationMinutes = null,
    int? CreditsPrice = null,
    SessionLocationType? LocationType = null,
    string? LocationDetails = null,
    int? MaxParticipants = null);
