using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Instructors.Commands.AddInstructorAvailability;

[AuthenticatedOnly]
public sealed record AddInstructorAvailabilityCommand(
    Guid InstructorProfileId,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime
) : ICommand<Guid>, ICacheInvalidatorCommand
{
    // بنمسح الكاش عشان الطالب يشوف المواعيد الجديدة فوراً
    public IReadOnlyCollection<string> CacheKeys =>
    [
        $"instructor-full-profile-userid-{InstructorProfileId}",
        "instructors-list-approved"
    ];
}