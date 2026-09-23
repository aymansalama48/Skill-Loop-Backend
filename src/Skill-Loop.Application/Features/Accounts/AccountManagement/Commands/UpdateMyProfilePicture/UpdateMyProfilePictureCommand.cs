using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfilePicture;

public sealed record UpdateMyProfilePictureCommand(
    Stream FileStream,
    string FileName) : ICommand<string>, ICacheInvalidatorCommand
{
    // مسح الكاش لظهور الصورة للآدمن
    public IReadOnlyCollection<string> CacheKeys => ["users-list"];
}