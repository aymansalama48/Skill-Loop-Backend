using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using System;
using System.Collections.Generic;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.UpdateLesson;

[Permission(Permissions.Courses.Update)]
public sealed record UpdateLessonCommand(
    Guid CourseId, 
    Guid SectionId, 
    Guid LessonId,
    string Title, 
    Stream? VideoStream, 
    string? VideoFileName, 
    TimeSpan Duration, 
    string? StreamingResolution, 
    string? ExternalProviderId, 
    int OrderIndex, 
    bool IsPreviewable) : ICommand, ICacheInvalidatorCommand, ICourseCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        AppCacheKeys.CoursesPrefix,
        AppCacheKeys.CoursesPaged,
        AppCacheKeys.CourseById(CourseId),
        AppCacheKeys.CourseById(CourseId)
    ];
}
