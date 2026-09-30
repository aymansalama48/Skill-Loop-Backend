using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Availability;

public static class AvailabilityErrors
{
    public static readonly Error InvalidTime = new Error(
        "Availability.InvalidTime",
        "Availability invalid time.",
        ErrorType.Validation);

}
