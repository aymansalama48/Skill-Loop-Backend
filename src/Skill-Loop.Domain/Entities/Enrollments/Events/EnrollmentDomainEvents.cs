using Skill_Loop.Domain.Common.Events;

namespace Skill_Loop.Domain.Entities.Enrollments.Events;

public sealed record CourseEnrolledDomainEvent(
    Guid EnrollmentId,
    Guid UserId,
    Guid CourseId,
    int CreditsPaid) : IDomainEvent;

public sealed record LessonCompletedDomainEvent(
    Guid EnrollmentId,
    Guid UserId,
    Guid CourseId,
    Guid LessonId) : IDomainEvent;

public sealed record CourseCompletedDomainEvent(
    Guid EnrollmentId,
    Guid UserId,
    Guid CourseId) : IDomainEvent;
