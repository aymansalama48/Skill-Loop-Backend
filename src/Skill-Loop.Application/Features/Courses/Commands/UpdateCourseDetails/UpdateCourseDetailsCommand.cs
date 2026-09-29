using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using Skill_Loop.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Skill_Loop.Application.Features.Courses.Commands.UpdateCourseDetails;

public sealed record UpdateCourseDetailsCommand(
    Guid CourseId,
    string Title,
    string Description,
    string ThumbnailUrl,
    int Credits,
    CourseLevel Level,
    Guid CategoryId) : ICommand, ICacheInvalidatorCommand, ICourseCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        "courses:paged:",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}"
    ];
}
