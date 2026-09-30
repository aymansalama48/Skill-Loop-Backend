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
        "courses:all",
        "courses:paged:",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}",
        $"course-reviews:{CourseId}"
    ];
}