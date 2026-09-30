using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Section;

public static class SectionErrors
{
    public static readonly Error EmptyTitle = new Error(
        "Section.EmptyTitle",
        "Section is required.",
        ErrorType.Validation);

    public static readonly Error NotFound = new Error(
        "Section.NotFound",
        "Section not found.",
        ErrorType.NotFound);

}
