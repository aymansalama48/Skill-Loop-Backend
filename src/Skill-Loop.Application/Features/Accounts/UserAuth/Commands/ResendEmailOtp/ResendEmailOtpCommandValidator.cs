using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.UserAuth.Commands.ResendEmailOtp;

public sealed class ResendEmailOtpCommandValidator : AbstractValidator<ResendEmailOtpCommand>
{
    public ResendEmailOtpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("بريد إلكتروني غير صالح.");
    }
}