namespace Skill_Loop.Application.Features.Otps.Commands.RequestOtp;

using FluentValidation;

public sealed class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("صيغة رقم الهاتف غير صحيحة");

        RuleFor(x => x.Purpose)
            .IsInEnum().WithMessage("الغرض من طلب الـ OTP غير صالح");
    }
}