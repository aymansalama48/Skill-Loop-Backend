using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Instructors.Commands.UpdateMyInstructorProfile;

// أضفنا الـ UserId للـ Command عشان نقدر نمسح الكاش الخاص بيه
public sealed record UpdateMyInstructorProfileCommand(
    Guid UserId,
    string Headline,
    string Bio
) : ICommand<bool>, ICacheInvalidatorCommand
{
    // بنمسح كاش اللستة، وكاش البروفايل الكامل، وكاش البروفايل المختصر
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "instructors-list-approved",
        $"instructor-full-profile-userid-{UserId}",
        $"instructor-profile-userid-{UserId}"
    ];
}