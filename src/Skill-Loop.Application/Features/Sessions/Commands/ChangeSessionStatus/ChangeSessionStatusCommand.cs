using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Commands.ChangeSessionStatus;

public sealed record ChangeSessionStatusCommand(
    Guid Id,
    SessionStatus NewStatus) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "sessions:all",
        $"sessions:{Id}"
    ];
}