using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Section;

public static class SectionErrors
{
    public static readonly Error NotFound = new Error(
        "Section.NotFound",
        "Section was not found.",
        ErrorType.NotFound);

}
