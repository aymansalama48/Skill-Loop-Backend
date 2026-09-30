using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorAvailability;

[AuthenticatedOnly]
public sealed record RemoveInstructorAvailabilityCommand(
    Guid InstructorProfileId,
    Guid AvailabilityId
) : ICommand<bool>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        $"instructor-full-profile-userid-{InstructorProfileId}",
        "instructors-list-approved"
    ];
}