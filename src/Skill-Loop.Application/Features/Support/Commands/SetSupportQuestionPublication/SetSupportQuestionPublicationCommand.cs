using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Commands.SetSupportQuestionPublication;

/// <summary>
/// ظ†ط´ط± ط³ط¤ط§ظ„ ظپظٹ ط§ظ„ظ€ FAQ ط§ظ„ط¹ط§ظ… ط£ظˆ ط¥ط®ظپط§ط¤ظ‡ ظ…ظ†ظ‡. ط§ظ„ظ†ط´ط± ظ…ط³طھط­ظٹظ„ ظ…ظ† ط؛ظٹط± ط¥ط¬ط§ط¨ط©.
/// </summary>
[Permission(Permissions.Support.Manage)]
public sealed record SetSupportQuestionPublicationCommand(
    Guid Id,
    bool IsPublished) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
