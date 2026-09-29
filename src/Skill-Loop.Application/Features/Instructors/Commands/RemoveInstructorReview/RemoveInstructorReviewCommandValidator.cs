using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorReview;

public sealed class RemoveInstructorReviewCommandValidator : AbstractValidator<RemoveInstructorReviewCommand>
{
    public RemoveInstructorReviewCommandValidator()
    {
        RuleFor(x => x.InstructorProfileId).NotEmpty().WithMessage("معرّف المدرب مطلوب.");
        RuleFor(x => x.ReviewId).NotEmpty().WithMessage("معرّف التقييم مطلوب.");
    }
}