using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Sessions.Common;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Commands.DeleteSession;

// Merge note: both sides are kept, and both are required.
//   - Sessions.Delete is origin/main's permission gate and replaces [AuthenticatedOnly].
//   - ISessionCommand is what feeds SessionOwnershipBehavior, which verifies the caller owns
//     the session (or holds Sessions.ManageAll). origin/main dropped the interface, which
//     would have let any user holding Sessions.Delete delete another instructor's session.
[Permission(Permissions.Sessions.Delete)]
public sealed record DeleteSessionCommand(Guid Id) : ICommand, ICacheInvalidatorCommand, ISessionCommand
{
    public Guid SessionId => Id;

    public IReadOnlyCollection<string> CacheKeys =>
    [
        "sessions:all",
        $"sessions:{Id}"
    ];
}