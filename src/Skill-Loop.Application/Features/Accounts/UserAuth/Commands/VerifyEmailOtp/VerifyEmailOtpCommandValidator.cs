using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;

public sealed class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
{
    public VerifyEmailOtpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("بريد إلكتروني غير صالح.");
        RuleFor(x => x.OtpCode).NotEmpty().Length(4).WithMessage("كود التحقق يجب أن يكون 4 أرقام.");
    }
}