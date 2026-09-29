using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Instructors.Commands.AddInstructorAvailability;

public sealed record AddInstructorAvailabilityCommand(
    Guid InstructorProfileId,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime
) : ICommand<bool>, ICacheInvalidatorCommand
{
    // بنمسح الكاش عشان الطالب يشوف المواعيد الجديدة فوراً
    public IReadOnlyCollection<string> CacheKeys =>
    [
        $"instructor-full-profile-userid-{InstructorProfileId}",
        "instructors-list-approved"
    ];
}