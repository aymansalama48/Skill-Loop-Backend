using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Courses.Commands.AddCourseReview;

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