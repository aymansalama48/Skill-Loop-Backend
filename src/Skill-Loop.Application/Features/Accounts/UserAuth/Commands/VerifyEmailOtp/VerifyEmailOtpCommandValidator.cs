using FluentValidation;
using Skill_Loop.Application.Common.Validation;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;

public sealed class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
{
    public VerifyEmailOtpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid value.");
        // Was .Length(4), which rejected every code the generator actually issues
        // (OtpSettings:CodeLength = 6).
        RuleFor(x => x.OtpCode).ApplyOtpCodeRules();
    }
}