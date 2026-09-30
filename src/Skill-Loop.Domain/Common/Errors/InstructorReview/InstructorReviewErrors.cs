using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.InstructorReview;

public static class InstructorReviewErrors
{
    public static readonly Error NotFound = new Error(
        "InstructorReview.NotFound",
        "InstructorReview was not found.",
        ErrorType.NotFound);

    public static readonly Error Unauthorized = new Error(
        "InstructorReview.Unauthorized",
        "InstructorReview unauthorized.",
        ErrorType.Unauthorized);

    public static readonly Error InvalidRating = new Error(
        "InstructorReview.InvalidRating",
        "InstructorReview invalid rating.",
        ErrorType.Validation);

}
