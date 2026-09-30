using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("This field is required.")
            .EmailAddress().WithMessage("Invalid email address format.");
    }
}