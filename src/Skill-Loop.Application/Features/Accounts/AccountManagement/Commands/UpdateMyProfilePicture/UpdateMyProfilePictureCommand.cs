using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfilePicture;

public sealed record UpdateMyProfilePictureCommand(string AvatarUrl) : ICommand<bool>, ICacheInvalidatorCommand
{
    // هيمسح كاش شاشة الـ CRM عشان صورته الجديدة تظهر للآدمن
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}