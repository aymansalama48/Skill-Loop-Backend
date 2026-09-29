using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Contracts.Sessions;

public sealed record CreateSessionRequest(
    string Title,
    string? Description = null,
    DateTime? ScheduledAtUtc = null,
    int DurationMinutes = 60,
    int CreditsPrice = 0,
    SessionLocationType LocationType = SessionLocationType.Online,
    string? LocationDetails = null,
    int MaxParticipants = 1);
