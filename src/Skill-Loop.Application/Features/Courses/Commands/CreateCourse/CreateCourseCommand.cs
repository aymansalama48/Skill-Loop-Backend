using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.CreateCourse;

[Permission(Permissions.Courses.Create)]
public sealed record CreateCourseCommand(
    string Title,
    string Description,
    int Credits,
    CourseLevel Level,
    Guid InstructorId,
    string InstructorName,
    Guid CategoryId) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        AppCacheKeys.CoursesPrefix,
        AppCacheKeys.CoursesPaged,
        AppCacheKeys.CoursesByCategory(CategoryId),
        AppCacheKeys.CoursesByInstructor(InstructorId)
    ];
}
