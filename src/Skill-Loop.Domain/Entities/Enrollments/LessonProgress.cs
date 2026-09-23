using Skill_Loop.Domain.Common.Entities;

namespace Skill_Loop.Domain.Entities.Enrollments;

public sealed class LessonProgress : BaseEntity
{
    public Guid EnrollmentId { get; private set; }
    public Guid LessonId { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private LessonProgress() { }

    internal static LessonProgress Create(Guid enrollmentId, Guid lessonId) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            EnrollmentId = enrollmentId,
            LessonId = lessonId,
            IsCompleted = false
        };

    internal void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }
}
