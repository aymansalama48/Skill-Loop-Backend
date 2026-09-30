using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.ChangeSessionStatus;

public sealed class ChangeSessionStatusCommandValidator : AbstractValidator<ChangeSessionStatusCommand>
{
    public ChangeSessionStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid value.");
    }
}