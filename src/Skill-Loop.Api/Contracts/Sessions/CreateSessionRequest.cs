using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Contracts.Sessions;

public sealed record CreateSessionRequest(
    string Title,
    int PriceInCredits,
    int DurationInMinutes,
    SessionType Type
);