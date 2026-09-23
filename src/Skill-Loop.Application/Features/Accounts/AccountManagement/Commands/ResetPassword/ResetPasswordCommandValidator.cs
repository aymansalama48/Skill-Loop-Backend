using FluentValidation;
using Skill_Loop.Application.Common.Validation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.OtpCode) // 👈 التحقق من كود الـ OTP
            .NotEmpty().WithMessage("كود التحقق مطلوب.")
            .Length(4).WithMessage("كود التحقق يجب أن يكون 4 أرقام.");

        RuleFor(x => x.NewPassword)
            .ApplyStandardPasswordRules();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("تأكيد كلمة المرور مطلوب.")
            .Equal(x => x.NewPassword).WithMessage("كلمة المرور وتأكيدها غير متطابقين.");
    }
}