using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.AddLesson;

[Permission(Permissions.Courses.Create)]
public sealed record AddLessonCommand(
    Guid CourseId,
    Guid SectionId,
    string Title,
    Stream? VideoStream,
    string? VideoFileName,
    TimeSpan Duration,
    int OrderIndex,
    bool IsPreviewable,
    string? StreamingResolution,
    string? ExternalProviderId) : ICommand<Guid>, ICacheInvalidatorCommand, ICourseCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        AppCacheKeys.CoursesPrefix,
        AppCacheKeys.CoursesPaged,
        AppCacheKeys.CourseById(CourseId),
        AppCacheKeys.CourseById(CourseId),
        AppCacheKeys.CourseSections(CourseId),
        AppCacheKeys.CourseLessons(CourseId)
    ];
}
