using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.ChangeSessionStatus;

public sealed class ChangeSessionStatusCommandValidator : AbstractValidator<ChangeSessionStatusCommand>
{
    public ChangeSessionStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف الجلسة مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف الجلسة غير صالح.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("حالة الجلسة غير صالحة.");
    }
}