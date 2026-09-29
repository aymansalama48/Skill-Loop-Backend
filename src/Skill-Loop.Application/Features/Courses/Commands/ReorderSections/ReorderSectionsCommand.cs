using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using System;
using System.Collections.Generic;

namespace Skill_Loop.Application.Features.Courses.Commands.ReorderSections;

public sealed record ReorderSectionsCommand(Guid CourseId, Dictionary<Guid, int> SectionOrders) : ICommand, ICacheInvalidatorCommand, ICourseCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        "courses:paged:",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}"
    ];
}
