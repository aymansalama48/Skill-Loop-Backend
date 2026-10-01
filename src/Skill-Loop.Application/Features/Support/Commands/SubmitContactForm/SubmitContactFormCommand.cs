using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Commands.SubmitContactForm;

/// <summary>
/// ط§ط³طھظپط³ط§ط± ط¬ط¯ظٹط¯ ظ…ظ† ظ…ط³طھط®ط¯ظ… ظ…ط³ط¬ظ‘ظ„ ط¯ط®ظˆظ„ظ‡.
/// ط§ط³ظ… ط§ظ„ظ…ط³طھط®ط¯ظ… ظˆط¨ط±ظٹط¯ظ‡ ط¨ظٹطھظ‚ط±ط¤ط§ ظ…ظ† ط§ظ„ظ€ JWT (ظ…ط´ ظ…ظ† ط§ظ„ظ€ Body) ط¹ط´ط§ظ† ظ…ط­ط¯ط´ ظٹظ‚ط¯ط± ظٹط¨ط¹طھ ط§ط³طھظپط³ط§ط± ط¨ط§ط³ظ… ط­ط¯ طھط§ظ†ظٹ.
/// </summary>
[AllowAnonymous]
public sealed record SubmitContactFormCommand(
    string Subject,
    string Message,
    string Category = "General") : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
