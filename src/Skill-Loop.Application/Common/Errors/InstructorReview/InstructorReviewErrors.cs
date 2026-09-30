using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.InstructorReview;

public static class InstructorReviewErrors
{
    public static readonly Error CannotReviewSelf = new Error(
        "InstructorReview.CannotReviewSelf",
        "InstructorReview cannot review self.",
        ErrorType.Conflict);

}
