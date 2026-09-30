using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Lesson;

public static class LessonErrors
{
    public static readonly Error EmptyVideoUrl = new Error(
        "Lesson.EmptyVideoUrl",
        "Lesson is required.",
        ErrorType.Validation);

    public static readonly Error InvalidDuration = new Error(
        "Lesson.InvalidDuration",
        "Lesson invalid duration.",
        ErrorType.Validation);

    public static readonly Error NotFound = new Error(
        "Lesson.NotFound",
        "Lesson not found.",
        ErrorType.NotFound);

}
