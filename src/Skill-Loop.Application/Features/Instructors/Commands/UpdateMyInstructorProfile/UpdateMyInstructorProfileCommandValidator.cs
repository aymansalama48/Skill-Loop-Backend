using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Commands.UpdateMyInstructorProfile;

public sealed class UpdateMyInstructorProfileCommandValidator : AbstractValidator<UpdateMyInstructorProfileCommand>
{
    public UpdateMyInstructorProfileCommandValidator()
    {
        RuleFor(x => x.Headline)
            .NotEmpty().WithMessage("العنوان التعريفي مطلوب.")
            .MaximumLength(200).WithMessage("العنوان التعريفي لا يمكن أن يتجاوز 200 حرف.");

        RuleFor(x => x.Bio)
            .NotEmpty().WithMessage("النبذة التعريفية مطلوبة.")
            .MaximumLength(2000).WithMessage("النبذة التعريفية لا يمكن أن تتجاوز 2000 حرف.");
    }
}