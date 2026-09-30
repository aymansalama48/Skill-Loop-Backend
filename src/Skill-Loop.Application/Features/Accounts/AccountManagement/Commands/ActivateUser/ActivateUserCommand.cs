using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ActivateUser;

[AuthenticatedOnly]
public sealed record ActivateUserCommand(Guid UserId) : ICommand<bool>, ICacheInvalidatorCommand
{
    // هيمسح كاش شاشة الـ CRM عشان حالة اليوزر اتغيرت
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}