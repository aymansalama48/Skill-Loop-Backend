using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Instructors.Commands.ChangeInstructorApprovalStatus;

//[Permission(Permissions.Instructors.ManageAll)]
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
