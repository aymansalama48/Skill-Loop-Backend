using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Sessions.Commands.CreateSession;

public sealed record CreateSessionCommand(
    string Title,
    Guid InstructorId,
    int PriceInCredits,
    int DurationInMinutes,
    SessionType Type) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => ["sessions:all"];
}