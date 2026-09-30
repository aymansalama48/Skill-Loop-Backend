using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.RemoveRoleFromUser;

[AuthenticatedOnly]
public sealed record RemoveRoleFromUserCommand(Guid UserId, string RoleName) : ICommand<bool>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}