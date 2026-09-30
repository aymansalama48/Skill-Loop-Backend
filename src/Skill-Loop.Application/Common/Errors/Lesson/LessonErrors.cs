using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Lesson;

public static class LessonErrors
{
    public static readonly Error NotFound = new Error(
        "Lesson.NotFound",
        "Lesson was not found.",
        ErrorType.NotFound);

}
