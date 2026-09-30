using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Commands.AddInstructorReview;

public sealed class AddInstructorReviewCommandValidator : AbstractValidator<AddInstructorReviewCommand>
{
    public AddInstructorReviewCommandValidator()
    {
        RuleFor(x => x.InstructorProfileId)
            .NotEmpty().WithMessage("This field is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Value is out of range.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Length exceeds the maximum allowed.");
    }
}