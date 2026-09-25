using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Instructors.Commands.UpdateInstructorReview;

public sealed record UpdateInstructorReviewCommand(
    Guid InstructorProfileId,
    Guid ReviewId,
    Guid LearnerUserId,
    int Rating,
    string? Comment
) : ICommand<bool>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        $"instructor-full-profile-userid-{InstructorProfileId}", // أو ID المدرب حسب إعدادات الكاش عندك
        "instructors-list-approved"
    ];
}