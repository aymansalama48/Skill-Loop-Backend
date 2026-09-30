using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Sessions.Common;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Commands.ChangeSessionStatus;

[Permission(Permissions.Sessions.Moderate)]
public sealed record ChangeSessionStatusCommand(
    Guid Id,
    SessionStatus NewStatus) : ICommand, ICacheInvalidatorCommand, ISessionCommand
{
    public Guid SessionId => Id;

    public IReadOnlyCollection<string> CacheKeys =>
    [
        "sessions:all",
        $"sessions:{Id}"
    ];
}