using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Category;

public static class CategoryErrors
{
    public static readonly Error EmptyName = new Error(
        "Category.EmptyName",
        "Category is required.",
        ErrorType.Validation);

    public static readonly Error EmptySlug = new Error(
        "Category.EmptySlug",
        "Category is required.",
        ErrorType.Validation);

}
