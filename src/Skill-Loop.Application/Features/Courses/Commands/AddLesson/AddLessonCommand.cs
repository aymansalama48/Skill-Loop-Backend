using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Courses.Commands.AddLesson;

public sealed record AddLessonCommand(
    Guid CourseId,
    Guid SectionId,
    string Title,
    string VideoUrl,
    TimeSpan Duration,
    int OrderIndex,
    bool IsPreviewable,
    string? StreamingResolution,
    string? ExternalProviderId) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        "courses:paged:",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}",
        $"course-sections:{CourseId}",
        $"course-lessons:{CourseId}"
    ];
}