using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffLogin;

public sealed class StaffLoginCommandValidator : AbstractValidator<StaffLoginCommand>
{
    public StaffLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("This field is required.")
            .EmailAddress().WithMessage("Invalid email address format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("This field is required.");
    }
}