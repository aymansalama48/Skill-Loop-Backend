using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.UserGoogleLogin;

public sealed class UserGoogleLoginCommandValidator : AbstractValidator<UserGoogleLoginCommand>
{
    public UserGoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("رمز Google (IdToken) مطلوب");
    }
}