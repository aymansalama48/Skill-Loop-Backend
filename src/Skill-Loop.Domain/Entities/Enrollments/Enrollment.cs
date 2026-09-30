using Skill_Loop.Domain.Common.Errors.Enrollment;
using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Enrollments.Events;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Enrollments;

public sealed class Enrollment : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public int CreditsPaid { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public double ProgressPercentage { get; private set; }
    public Guid? LastWatchedLessonId { get; private set; }
    public DateTime EnrolledAt { get; private set; }

    private readonly List<LessonProgress> _lessonProgresses = [];
    public IReadOnlyCollection<LessonProgress> LessonProgresses => _lessonProgresses.AsReadOnly();

    private Enrollment() { }

    public static Result<Enrollment> Create(Guid userId, Guid courseId, int creditsPaid, int totalCourseLessons)
    {
        if (userId == Guid.Empty)
            return Result<Enrollment>.Failure(EnrollmentErrors.InvalidUser);

        if (courseId == Guid.Empty)
            return Result<Enrollment>.Failure(EnrollmentErrors.InvalidCourse);

        var enrollment = new Enrollment
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            CourseId = courseId,
            CreditsPaid = creditsPaid,
            Status = EnrollmentStatus.Active,
            ProgressPercentage = 0.0,
            EnrolledAt = DateTime.UtcNow
        };

        enrollment.AddDomainEvent(new CourseEnrolledDomainEvent(enrollment.Id, userId, courseId, creditsPaid));
        return Result<Enrollment>.Success(enrollment);
    }

    public Result MarkLessonCompleted(Guid lessonId, int totalCourseLessons)
    {
        if (lessonId == Guid.Empty)
            return Result.Failure(EnrollmentErrors.InvalidLesson);

        if (Status != EnrollmentStatus.Active)
            return Result.Failure(EnrollmentErrors.NotActive);

        if (totalCourseLessons <= 0)
            return Result.Failure(EnrollmentErrors.InvalidCourse);

        var progress = _lessonProgresses.FirstOrDefault(p => p.LessonId == lessonId);
        if (progress == null)
        {
            progress = LessonProgress.Create(Id, lessonId);
            _lessonProgresses.Add(progress);
        }

        progress.Complete();
        LastWatchedLessonId = lessonId;

        var completedCount = _lessonProgresses.Count(p => p.IsCompleted);

        // Clamp: progress rows can only ever reflect the lessons actually in the course,
        // and the stored value must never exceed 100.
        ProgressPercentage = Math.Clamp(
            Math.Round(((double)completedCount / totalCourseLessons) * 100, 2),
            0.0,
            100.0);

        if (ProgressPercentage >= 100 && Status != EnrollmentStatus.Completed)
        {
            Status = EnrollmentStatus.Completed;
            AddDomainEvent(new CourseCompletedDomainEvent(Id, UserId, CourseId));
        }

        AddDomainEvent(new LessonCompletedDomainEvent(Id, UserId, CourseId, lessonId));
        return Result.Success();
    }
}
