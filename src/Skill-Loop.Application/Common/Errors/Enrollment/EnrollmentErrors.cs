using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Enrollment;

public static class EnrollmentErrors
{
    public static readonly Error Duplicate = new Error(
        "Enrollment.Duplicate",
        "This enrollment is already duplicate.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new Error(
        "Enrollment.NotFound",
        "Enrollment was not found.",
        ErrorType.NotFound);

}
