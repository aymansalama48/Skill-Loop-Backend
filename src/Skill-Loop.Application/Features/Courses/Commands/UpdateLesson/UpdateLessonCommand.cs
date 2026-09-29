using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using System;
using System.Collections.Generic;

namespace Skill_Loop.Application.Features.Courses.Commands.UpdateLesson;

public sealed record UpdateLessonCommand(
    Guid CourseId, 
    Guid SectionId, 
    Guid LessonId,
    string Title, 
    string VideoUrl, 
    TimeSpan Duration, 
    string? StreamingResolution, 
    string? ExternalProviderId, 
    int OrderIndex, 
    bool IsPreviewable) : ICommand, ICacheInvalidatorCommand, ICourseCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        "courses:paged:",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}"
    ];
}
