using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorAvailability;

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