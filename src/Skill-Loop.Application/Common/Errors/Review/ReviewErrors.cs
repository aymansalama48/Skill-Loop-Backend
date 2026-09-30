using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Review;

public static class ReviewErrors
{
    public static readonly Error NotEnrolled = new Error(
        "Review.NotEnrolled",
        "Review not enrolled.",
        ErrorType.Validation);

    public static readonly Error BookingRequired = new Error(
        "Review.BookingRequired",
        "Review booking required.",
        ErrorType.Validation);

}
