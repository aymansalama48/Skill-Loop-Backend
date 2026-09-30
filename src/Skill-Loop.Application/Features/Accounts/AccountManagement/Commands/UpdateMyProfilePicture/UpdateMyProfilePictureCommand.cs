using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfilePicture;

[AuthenticatedOnly]
public sealed record UpdateMyProfilePictureCommand(
    Stream FileStream,
    string FileName) : ICommand<string>, ICacheInvalidatorCommand
{
    // مسح الكاش لظهور الصورة للآدمن
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}