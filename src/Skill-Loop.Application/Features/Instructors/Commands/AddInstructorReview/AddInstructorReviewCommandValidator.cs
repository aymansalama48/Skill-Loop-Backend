using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Commands.AddInstructorReview;

public sealed class AddInstructorReviewCommandValidator : AbstractValidator<AddInstructorReviewCommand>
{
    public AddInstructorReviewCommandValidator()
    {
        RuleFor(x => x.InstructorProfileId)
            .NotEmpty().WithMessage("معرّف المدرب مطلوب.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("التقييم يجب أن يكون بين 1 و 5 نجوم.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("التعليق يجب ألا يتجاوز 1000 حرف.");
    }
}