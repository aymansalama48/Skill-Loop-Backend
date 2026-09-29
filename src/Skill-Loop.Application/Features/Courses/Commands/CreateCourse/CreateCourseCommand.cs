using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Courses.Commands.CreateCourse;

public sealed record CreateCourseCommand(
    string Title,
    string Description,
    string ThumbnailUrl,
    int Credits,
    CourseLevel Level,
    Guid InstructorId,
    string InstructorName,
    Guid CategoryId) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        "courses:paged:",
        $"courses:category:{CategoryId}",
        $"courses:instructor:{InstructorId}"
    ];
}