using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.Review;

public static class ReviewErrors
{
    public static readonly Error InvalidStars = new Error(
        "Review.InvalidStars",
        "Review invalid stars.",
        ErrorType.Validation);

    public static readonly Error InstructorCannotReview = new Error(
        "Review.InstructorCannotReview",
        "Review instructor cannot review.",
        ErrorType.Validation);

    public static readonly Error Duplicate = new Error(
        "Review.Duplicate",
        "This review is already duplicate.",
        ErrorType.Validation);

}
