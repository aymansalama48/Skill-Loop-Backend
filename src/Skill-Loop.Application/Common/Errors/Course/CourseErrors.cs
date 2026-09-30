using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Course;

public static class CourseErrors
{
    public static readonly Error NotFound = new Error(
        "Course.NotFound",
        "Course not found.",
        ErrorType.NotFound);

    public static readonly Error Forbidden = new Error(
        "Course.Forbidden",
        "Course forbidden.",
        ErrorType.Forbidden);

}
