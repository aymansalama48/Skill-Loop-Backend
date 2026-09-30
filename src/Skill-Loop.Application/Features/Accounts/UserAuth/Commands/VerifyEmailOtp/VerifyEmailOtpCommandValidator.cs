using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;

public sealed class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
{
    public VerifyEmailOtpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid value.");
        RuleFor(x => x.OtpCode).NotEmpty().Length(4).WithMessage("Invalid value.");
    }
}