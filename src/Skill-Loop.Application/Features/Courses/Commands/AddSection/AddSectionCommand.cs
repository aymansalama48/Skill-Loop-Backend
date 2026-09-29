using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Courses.Commands.AddSection;

public sealed record AddSectionCommand(
    Guid CourseId,
    string Title,
    int OrderIndex) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}",
        $"course-sections:{CourseId}"
    ];
}