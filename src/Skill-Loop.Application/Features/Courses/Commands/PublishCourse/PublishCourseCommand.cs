using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Courses.Commands.PublishCourse;

public sealed record PublishCourseCommand(Guid CourseId) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        "courses:paged:",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}",
        "courses:published"
    ];
}