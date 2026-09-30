using FluentValidation;
using Skill_Loop.Application.Common.Validation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("This field is required.")
            .EmailAddress().WithMessage("Invalid email address format.");

        // 👈 التحقق من كود الـ OTP
        //   كان .Length(4) هنا أيضًا، فأي كود حقيقي (6 أرقام) كان مرفوضًا
        RuleFor(x => x.OtpCode)
            .ApplyOtpCodeRules();

        RuleFor(x => x.NewPassword)
            .ApplyStandardPasswordRules();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("This field is required.")
            .Equal(x => x.NewPassword).WithMessage("Invalid value.");
    }
}