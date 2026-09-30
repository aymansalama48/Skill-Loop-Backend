using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfile;

[AuthenticatedOnly]
public sealed record UpdateMyAccountProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber
) : ICommand<bool>, ICacheInvalidatorCommand
{
    // هيمسح كاش شاشة الـ CRM عشان لو الآدمن فاتح اللستة يشوف اسمه أو رقمه الجديد
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}