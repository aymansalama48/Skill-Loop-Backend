using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Commands.UpdateSupportQuestion;

/// <summary>
/// طھط¹ط¯ظٹظ„ ظƒط§ظ…ظ„ ظ„ط³ط¤ط§ظ„ ظ‚ط§ظٹظ… (ط§ظ„ط³ط¤ط§ظ„طŒ ط§ظ„ط±ط¯طŒ ط§ظ„طھطµظ†ظٹظپطŒ ط­ط§ظ„ط© ط§ظ„ظ†ط´ط±).
/// </summary>
[Permission(Permissions.Support.Manage)]
public sealed record UpdateSupportQuestionCommand(
    Guid Id,
    string Question,
    string? Answer,
    string Category,
    bool IsPublished) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
