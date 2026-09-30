using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Commands.CreateSession;

[Permission(Permissions.Sessions.Create)]
public sealed record CreateSessionCommand(
    string Title,
    Guid InstructorId,
    string? Description = null,
    DateTime? ScheduledAtUtc = null,
    int DurationMinutes = SessionDefaults.DurationMinutes,
    int CreditsPrice = 0,
    SessionLocationType LocationType = SessionLocationType.Online,
    string? LocationDetails = null,
    int MaxParticipants = 1) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => ["sessions:all"];
}
