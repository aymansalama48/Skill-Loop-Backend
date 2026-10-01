using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Commands.AnswerSupportQuestion;

/// <summary>
/// ط±ط¯ ظپط±ظٹظ‚ ط§ظ„ط¯ط¹ظ… ط¹ظ„ظ‰ ط§ط³طھظپط³ط§ط± ظ‚ط§ظٹظ…. ظ„ظˆ ط§ظ„ظ€ Publish = true ط§ظ„ط³ط¤ط§ظ„ ط¨ظٹظ†ط²ظ„ ظپظٹ ط§ظ„ظ€ FAQ ط§ظ„ط¹ط§ظ… ط¨ط¹ط¯ ظ…ط§ ظٹطھط¨ط¹طھ ظ„ظ„ظ…ط³طھط®ط¯ظ….
/// </summary>
[Permission(Permissions.Support.Manage)]
public sealed record AnswerSupportQuestionCommand(
    Guid Id,
    string Answer,
    bool Publish = false) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys => SupportCacheKeys.All;
}
