using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.CourseMaterial;

public static class CourseMaterialErrors
{
    public static readonly Error NotFound = new Error(
        "CourseMaterial.NotFound",
        "CourseMaterial was not found.",
        ErrorType.NotFound);

}
