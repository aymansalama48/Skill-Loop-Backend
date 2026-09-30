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
    string VideoUrl,
    TimeSpan Duration,
    int OrderIndex,
    bool IsPreviewable,
    string? StreamingResolution,
    string? ExternalProviderId) : ICommand<Guid>, ICacheInvalidatorCommand, ICourseCommand
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