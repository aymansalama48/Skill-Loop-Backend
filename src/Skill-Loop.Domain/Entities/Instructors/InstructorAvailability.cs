using Skill_Loop.Domain.Common.Errors.Availability;
using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Entities.Instructors;

// استخدمنا BaseEntity لأن إدارته ستتم عبر الـ Profile
public sealed class InstructorAvailability : BaseEntity
{
    public Guid InstructorProfileId { get; private set; } // الربط المباشر بالبروفايل
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }

    private InstructorAvailability() { }

    // Internal حتى لا يتم إنشاؤه إلا من خلال الـ InstructorProfile
    internal static Result<InstructorAvailability> Create(
        Guid instructorProfileId,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        if (startTime >= endTime)
            return Result<InstructorAvailability>.Failure(AvailabilityErrors.InvalidTime);

        return Result<InstructorAvailability>.Success(new InstructorAvailability
        {
            Id = Guid.CreateVersion7(),
            InstructorProfileId = instructorProfileId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime
        });
    }

    internal Result UpdateTime(TimeSpan newStartTime, TimeSpan newEndTime)
    {
        if (newStartTime >= newEndTime)
            return Result.Failure(AvailabilityErrors.InvalidTime);

        StartTime = newStartTime;
        EndTime = newEndTime;

        return Result.Success();
    }
}