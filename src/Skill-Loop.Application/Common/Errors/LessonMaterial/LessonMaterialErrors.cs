using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.LessonMaterial;

public static class LessonMaterialErrors
{
    public static readonly Error NotFound = new Error(
        "LessonMaterial.NotFound",
        "LessonMaterial was not found.",
        ErrorType.NotFound);

}
