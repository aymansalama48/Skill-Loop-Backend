using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Instructors.Commands.ChangeInstructorApprovalStatus;

// Merge note: origin/main arrived with this attribute commented out
// (//[Permission(Permissions.Instructors.ManageAll)]). Commenting out an authorization
// attribute removes the check entirely rather than relaxing it, so anyone able to reach
// this command could approve instructors. Left enabled - see docs/SECURITY.md.
[Permission(Permissions.Instructors.ManageAll)]
public sealed record ChangeInstructorApprovalStatusCommand(
    Guid UserId,
    bool IsApproved
) : ICommand<bool>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "instructors-list-approved",
        $"instructor-full-profile-userid-{UserId}",
        $"instructor-profile-userid-{UserId}"
    ];
}
