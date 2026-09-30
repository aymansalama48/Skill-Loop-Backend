using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("This field is required.");
    }
}