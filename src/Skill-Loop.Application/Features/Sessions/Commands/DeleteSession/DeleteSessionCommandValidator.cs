using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.DeleteSession;

public sealed class DeleteSessionCommandValidator : AbstractValidator<DeleteSessionCommand>
{
    public DeleteSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");
    }
}