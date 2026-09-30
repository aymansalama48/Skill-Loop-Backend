using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfile;

public sealed class UpdateMyAccountProfileCommandValidator : AbstractValidator<UpdateMyAccountProfileCommand>
{
    public UpdateMyAccountProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("This field is required.")
            .MaximumLength(50).WithMessage("Length exceeds the maximum allowed.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("This field is required.")
            .MaximumLength(50).WithMessage("Length exceeds the maximum allowed.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Invalid value.");
    }
}