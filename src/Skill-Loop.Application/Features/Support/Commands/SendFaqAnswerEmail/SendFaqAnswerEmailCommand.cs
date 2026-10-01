using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Commands.SendFaqAnswerEmail;

/// <summary>
/// ط§ظ„ظ…ط³طھط®ط¯ظ… ظ„ظ‚ظ‰ ط¥ط¬ط§ط¨طھظ‡ ظپظٹ ط§ظ„ظ€ FAQ ظˆط·ظ„ط¨ ط¥ط±ط³ط§ظ„ظ‡ط§ ظ„ظٹظ‡ ط¹ظ„ظ‰ ط§ظ„ط¥ظٹظ…ظٹظ„.
/// ط¨ظٹط´طھط؛ظ„ ط¹ظ„ظ‰ ط³ط¤ط§ظ„ ظ…ظ†ط´ظˆط± ط¨ط³ (ط¹ط´ط§ظ† ظ…ظپظٹط´ ط­ط¯ ظٹط·ظ„ط¨ ط¥ط¬ط§ط¨ط© ط³ط¤ط§ظ„ ظ„ط³ظ‡ ظ…طھط¬ط§ظˆط¨ط´ ط¹ظ„ظٹظ‡).
/// </summary>
[Permission(Permissions.Support.Manage)]
public sealed record SendFaqAnswerEmailCommand(Guid Id) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => [SupportCacheKeys.QuestionById + Id];
}
