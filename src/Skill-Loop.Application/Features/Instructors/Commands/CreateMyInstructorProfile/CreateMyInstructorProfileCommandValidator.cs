using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Commands.CreateMyInstructorProfile;

public sealed class CreateMyInstructorProfileCommandValidator : AbstractValidator<CreateMyInstructorProfileCommand>
{
    public CreateMyInstructorProfileCommandValidator()
    {
        RuleFor(x => x.Headline)
            .NotEmpty().WithMessage("This field is required.")
            .MaximumLength(200).WithMessage("Length exceeds the maximum allowed.");

        RuleFor(x => x.Bio)
            .NotEmpty().WithMessage("This field is required.")
            .MaximumLength(2000).WithMessage("Invalid value.");
    }
}