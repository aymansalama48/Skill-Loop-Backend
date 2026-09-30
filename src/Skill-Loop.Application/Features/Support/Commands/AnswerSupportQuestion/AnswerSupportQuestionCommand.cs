using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Commands.AnswerSupportQuestion;

/// <summary>
/// رد فريق الدعم على استفسار قايم. لو الـ Publish = true السؤال بينزل في الـ FAQ العام بعد ما يتبعت للمستخدم.
/// </summary>
[AuthenticatedOnly]
public sealed record AnswerSupportQuestionCommand(
    Guid Id,
    string Answer,
    bool Publish = false) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
