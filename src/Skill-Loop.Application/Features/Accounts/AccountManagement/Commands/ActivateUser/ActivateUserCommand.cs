using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ActivateUser;

public sealed record ActivateUserCommand(Guid UserId) : ICommand<bool>, ICacheInvalidatorCommand
{
    // هيمسح كاش شاشة الـ CRM عشان حالة اليوزر اتغيرت
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}