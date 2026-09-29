using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;

namespace Skill_Loop.Application.Features.Support.Commands.SetSupportQuestionPublication;

/// <summary>
/// نشر سؤال في الـ FAQ العام أو إخفاؤه منه. النشر مستحيل من غير إجابة.
/// </summary>
public sealed record SetSupportQuestionPublicationCommand(
    Guid Id,
    bool IsPublished) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
