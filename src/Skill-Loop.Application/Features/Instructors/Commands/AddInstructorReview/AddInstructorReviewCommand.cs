using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Instructors.Commands.AddInstructorReview;

// بنبعت الـ InstructorProfileId عشان نمسح كاش البروفايل ده تحديداً، والـ UserId (الطالب)
public sealed record AddInstructorReviewCommand(
    Guid InstructorProfileId,
    Guid LearnerUserId,
    int Rating,
    string? Comment
) : ICommand<Guid>, ICacheInvalidatorCommand
{
    // هنمسح الكاش بتاع البروفايل عشان التقييم الجديد يسمع فيه
    public IReadOnlyCollection<string> CacheKeys =>
    [
        $"instructor-full-profile-userid-{InstructorProfileId}", // المفروض يتعدل ليكون ProfileId بدل UserId أو نضيف الاثنين
        "instructors-list-approved" // بنمسح كاش اللستة عشان المتوسط الجديد يظهر
    ];
}