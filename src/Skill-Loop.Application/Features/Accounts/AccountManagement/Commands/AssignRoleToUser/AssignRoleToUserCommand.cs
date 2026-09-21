using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.AssignRoleToUser;

public sealed record AssignRoleToUserCommand(Guid UserId, string RoleName) : ICommand<bool>, ICacheInvalidatorCommand
{
    // هنمسح كاش الآدمن عشان اللستة تتحدث بالرول الجديد فوراً
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}