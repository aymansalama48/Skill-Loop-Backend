using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using System;
using System.Collections.Generic;

namespace Skill_Loop.Application.Features.Courses.Commands.DeleteCourse;

public sealed record DeleteCourseCommand(Guid CourseId) : ICommand, ICacheInvalidatorCommand, ICourseCommand
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
