using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Instructors.Commands.CreateMyInstructorProfile;

[AuthenticatedOnly]
public sealed record CreateMyInstructorProfileCommand(
    Guid UserId,
    string Headline,
    string Bio
) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "instructors-list-approved",
        $"instructor-profile-userid-{UserId}"
    ];
}