using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;

public sealed record UpdateSessionCommand(
    Guid Id,
    string Title,
    int PriceInCredits,
    int DurationInMinutes,
    SessionType Type) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "sessions:all",
        $"sessions:{Id}"
    ];
}