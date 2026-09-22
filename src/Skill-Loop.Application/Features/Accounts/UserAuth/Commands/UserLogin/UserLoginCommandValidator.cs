// LoginUserCommandValidator.cs
using FluentValidation;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.LoginUser;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.UserLogin;

public sealed class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("بريد إلكتروني غير صالح.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.");
    }
}