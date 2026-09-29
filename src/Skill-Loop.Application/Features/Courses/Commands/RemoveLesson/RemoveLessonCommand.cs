using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using System;
using System.Collections.Generic;

namespace Skill_Loop.Application.Features.Courses.Commands.RemoveLesson;

public sealed record RemoveLessonCommand(Guid CourseId, Guid SectionId, Guid LessonId) : ICommand, ICacheInvalidatorCommand, ICourseCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        "courses:paged:",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}"
    ];
}
