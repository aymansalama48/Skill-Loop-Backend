using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;

namespace Skill_Loop.Application.Features.Support.Commands.UpdateSupportQuestion;

/// <summary>
/// تعديل كامل لسؤال قايم (السؤال، الرد، التصنيف، حالة النشر).
/// </summary>
public sealed record UpdateSupportQuestionCommand(
    Guid Id,
    string Question,
    string? Answer,
    string Category,
    bool IsPublished) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
