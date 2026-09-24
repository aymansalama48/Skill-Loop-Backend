using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Sessions.Commands.CreateSession;

public sealed record CreateSessionCommand(
    string Title,
    Guid InstructorId) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => ["sessions:all"];
}