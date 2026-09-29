using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Contracts.Sessions;

public sealed record ChangeSessionStatusRequest(SessionStatus Status);