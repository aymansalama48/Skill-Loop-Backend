using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.PublishCourse;

[Permission(Permissions.Courses.Publish)]
public sealed record PublishCourseCommand(Guid CourseId) : ICommand, ICacheInvalidatorCommand, ICourseCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        AppCacheKeys.CoursesPrefix,
        AppCacheKeys.CoursesPaged,
        AppCacheKeys.CourseById(CourseId),
        AppCacheKeys.CourseById(CourseId),
        "courses:published"
    ];
}
