using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Commands.SetSupportQuestionPublication;

/// <summary>
/// نشر سؤال في الـ FAQ العام أو إخفاؤه منه. النشر مستحيل من غير إجابة.
/// </summary>
[Permission(Permissions.Support.Manage)]
public sealed record SetSupportQuestionPublicationCommand(
    Guid Id,
    bool IsPublished) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
