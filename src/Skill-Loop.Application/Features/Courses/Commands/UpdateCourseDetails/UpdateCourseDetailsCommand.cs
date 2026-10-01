using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using Skill_Loop.Domain.Enums;
using System;
using System.Collections.Generic;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.UpdateCourseDetails;

[Permission(Permissions.Courses.Update)]
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
        AppCacheKeys.CoursesPrefix,
        AppCacheKeys.CoursesPaged,
        AppCacheKeys.CourseById(CourseId),
        AppCacheKeys.CourseById(CourseId)
    ];
}
