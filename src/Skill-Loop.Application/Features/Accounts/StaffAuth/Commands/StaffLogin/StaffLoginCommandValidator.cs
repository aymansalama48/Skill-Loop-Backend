using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffLogin;

public sealed class StaffLoginCommandValidator : AbstractValidator<StaffLoginCommand>
{
    public StaffLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.");
    }
}