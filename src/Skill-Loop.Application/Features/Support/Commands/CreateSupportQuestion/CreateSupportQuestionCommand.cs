using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;

namespace Skill_Loop.Application.Features.Support.Commands.CreateSupportQuestion;

public sealed record CreateSupportQuestionCommand(
    string Question,
    string? Answer,
    string Category,
    bool IsPublished = false) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
