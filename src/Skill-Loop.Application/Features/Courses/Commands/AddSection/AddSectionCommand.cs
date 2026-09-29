using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;

namespace Skill_Loop.Application.Features.Courses.Commands.AddSection;

public sealed record AddSectionCommand(
    Guid CourseId,
    string Title,
    int OrderIndex) : ICommand<Guid>, ICacheInvalidatorCommand, ICourseCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}",
        $"course-sections:{CourseId}"
    ];
}