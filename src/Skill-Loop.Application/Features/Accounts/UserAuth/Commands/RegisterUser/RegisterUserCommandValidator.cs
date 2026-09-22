using FluentValidation;
using Skill_Loop.Application.Common.Validation;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("الاسم الأول مطلوب.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("اسم العائلة مطلوب.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("بريد إلكتروني غير صالح.");

        RuleFor(x => x.Password)
            .ApplyStandardPasswordRules();
    }
}