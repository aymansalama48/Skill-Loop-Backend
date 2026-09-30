using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Category;

public static class CategoryErrors
{
    public static readonly Error DuplicateSlug = new Error(
        "Category.DuplicateSlug",
        "This category is already duplicateslug.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new Error(
        "Category.NotFound",
        "Category was not found.",
        ErrorType.NotFound);

    public static readonly Error HasCourses = new Error(
        "Category.HasCourses",
        "Category has courses.",
        ErrorType.Conflict);

}
