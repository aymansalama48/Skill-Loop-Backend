using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.DeleteSession;

public sealed class DeleteSessionCommandValidator : AbstractValidator<DeleteSessionCommand>
{
    public DeleteSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف الجلسة مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف الجلسة غير صالح.");
    }
}