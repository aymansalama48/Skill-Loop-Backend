using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.InstructorProfile;

public static class InstructorProfileErrors
{
    public static readonly Error InvalidUser = new Error(
        "InstructorProfile.InvalidUser",
        "InstructorProfile invalid user.",
        ErrorType.Validation);

    public static readonly Error AlreadyReviewed = new Error(
        "InstructorProfile.AlreadyReviewed",
        "InstructorProfile already reviewed.",
        ErrorType.Conflict);

    public static readonly Error AvailabilityOverlap = new Error(
        "InstructorProfile.AvailabilityOverlap",
        "InstructorProfile availability overlap.",
        ErrorType.Conflict);

    public static readonly Error AvailabilityNotFound = new Error(
        "InstructorProfile.AvailabilityNotFound",
        "InstructorProfile was not found.",
        ErrorType.NotFound);

}
