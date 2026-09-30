using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.RegisterUser;

[AllowAnonymous]
public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : ICommand<bool>, ICacheInvalidatorCommand
{
    // مسح كاش قائمة المستخدمين لظهور المستخدم الجديد
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}