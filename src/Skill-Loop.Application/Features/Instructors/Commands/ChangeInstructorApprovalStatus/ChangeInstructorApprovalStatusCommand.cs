using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Instructors.Commands.ChangeInstructorApprovalStatus;

public sealed record ChangeInstructorApprovalStatusCommand(
    Guid UserId,
    bool IsApproved
) : ICommand<bool>, ICacheInvalidatorCommand
{
    // بنمسح كاش اللستة (عشان لو اعتمدناه يظهر، ولو وقفناه يختفي) وكاش البروفايل الخاص بيه
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "instructors-list-approved",
        $"instructor-full-profile-userid-{UserId}",
        $"instructor-profile-userid-{UserId}"
    ];
}