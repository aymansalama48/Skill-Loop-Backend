using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfile;

public sealed record UpdateMyAccountProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber
) : ICommand<bool>, ICacheInvalidatorCommand
{
    // هيمسح كاش شاشة الـ CRM عشان لو الآدمن فاتح اللستة يشوف اسمه أو رقمه الجديد
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}