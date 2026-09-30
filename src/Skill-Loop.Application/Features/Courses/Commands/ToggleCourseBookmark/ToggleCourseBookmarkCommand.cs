using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Commands.ToggleCourseBookmark;

[AuthenticatedOnly]
public sealed record ToggleCourseBookmarkCommand(
    Guid UserId,
    Guid CourseId) : ICommand<bool>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "courses:all",
        $"courses:{CourseId}",
        $"courses:detail:{CourseId}",
        $"user-bookmarks:{UserId}"
    ];
}