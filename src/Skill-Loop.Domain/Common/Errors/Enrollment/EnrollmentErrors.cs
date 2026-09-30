using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Enrollment;

public static class EnrollmentErrors
{
    public static readonly Error InvalidUser = new Error(
        "Enrollment.InvalidUser",
        "Enrollment is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCourse = new Error(
        "Enrollment.InvalidCourse",
        "Enrollment is required.",
        ErrorType.Validation);

    public static readonly Error InvalidLesson = new Error(
        "Enrollment.InvalidLesson",
        "Enrollment is required.",
        ErrorType.Validation);

}
