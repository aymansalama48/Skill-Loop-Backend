using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Course;

public static class CourseErrors
{
    public static readonly Error TitleEmpty = new Error(
        "Course.TitleEmpty",
        "Course is required.",
        ErrorType.Validation);

    public static readonly Error DescriptionEmpty = new Error(
        "Course.DescriptionEmpty",
        "Course is required.",
        ErrorType.Validation);

    public static readonly Error InvalidInstructor = new Error(
        "Course.InvalidInstructor",
        "Course is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCategory = new Error(
        "Course.InvalidCategory",
        "Course is required.",
        ErrorType.Validation);

    public static readonly Error NegativeCredits = new Error(
        "Course.NegativeCredits",
        "Course negative credits.",
        ErrorType.Validation);

    public static readonly Error MissingThumbnail = new Error(
        "Course.MissingThumbnail",
        "Course missing thumbnail.",
        ErrorType.Validation);

    public static readonly Error NoSections = new Error(
        "Course.NoSections",
        "Course no sections.",
        ErrorType.Validation);

    public static readonly Error EmptySections = new Error(
        "Course.EmptySections",
        "Course empty sections.",
        ErrorType.Validation);

    public static readonly Error NotFound = new Error(
        "Course.NotFound",
        "Course not found.",
        ErrorType.NotFound);
}
