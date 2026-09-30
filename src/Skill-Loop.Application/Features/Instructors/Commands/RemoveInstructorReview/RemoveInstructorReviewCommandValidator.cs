using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorReview;

public sealed class RemoveInstructorReviewCommandValidator : AbstractValidator<RemoveInstructorReviewCommand>
{
    public RemoveInstructorReviewCommandValidator()
    {
        RuleFor(x => x.InstructorProfileId).NotEmpty().WithMessage("This field is required.");
        RuleFor(x => x.ReviewId).NotEmpty().WithMessage("This field is required.");
    }
}