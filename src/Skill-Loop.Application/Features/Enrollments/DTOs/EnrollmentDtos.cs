namespace Skill_Loop.Application.Features.Enrollments.DTOs;

public sealed record EnrollmentResultDto(
    Guid EnrollmentId,
    Guid UserId,
    Guid CourseId,
    int CreditsPaid,
    string Status,
    DateTime EnrolledAt);

public sealed record UserEnrolledCourseDto(
    Guid EnrollmentId,
    Guid CourseId,
    string CourseTitle,
    string CourseThumbnailUrl,
    string InstructorName,
    double ProgressPercentage,
    string Status,
    Guid? LastWatchedLessonId,
    int TotalLessonsCount,
    DateTime EnrolledAt);
