using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Commands.DeleteSupportQuestion;

[AuthenticatedOnly]
public sealed record DeleteSupportQuestionCommand(Guid Id) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
