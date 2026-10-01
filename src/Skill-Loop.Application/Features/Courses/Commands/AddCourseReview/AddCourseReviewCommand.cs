using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.AddCourseReview;

[AuthenticatedOnly]
public sealed record AddCourseReviewCommand(
    Guid CourseId,
    Guid UserId,
    int Stars,
    string? Comment) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        AppCacheKeys.CoursesPrefix,
        AppCacheKeys.CoursesPaged,
        AppCacheKeys.CourseById(CourseId),
        AppCacheKeys.CourseById(CourseId),
        $"course-reviews:{CourseId}"
    ];
}
