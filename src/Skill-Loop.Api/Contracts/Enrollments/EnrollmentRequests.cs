namespace Skill_Loop.Api.Contracts.Enrollments;

public sealed record EnrollInCourseRequest(Guid CourseId);

public sealed record UpdateLessonProgressRequest(Guid LessonId);
