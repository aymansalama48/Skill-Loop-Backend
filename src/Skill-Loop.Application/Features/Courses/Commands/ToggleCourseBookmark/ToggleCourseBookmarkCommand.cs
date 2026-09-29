using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Courses.Commands.ToggleCourseBookmark;

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