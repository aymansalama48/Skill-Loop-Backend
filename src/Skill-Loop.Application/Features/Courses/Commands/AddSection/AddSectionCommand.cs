using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Courses.Common;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.AddSection;

[Permission(Permissions.Courses.Create)]
public sealed record AddSectionCommand(
    Guid CourseId,
    string Title,
    int OrderIndex) : ICommand<Guid>, ICacheInvalidatorCommand, ICourseCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}",
        $"course-sections:{CourseId}"
    ];
}