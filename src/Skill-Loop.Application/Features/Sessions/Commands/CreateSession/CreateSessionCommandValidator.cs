using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.CreateSession;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان الجلسة مطلوب.")
            .MaximumLength(200).WithMessage("عنوان الجلسة لا يجب أن يتجاوز 200 حرف.");

        RuleFor(x => x.InstructorId)
            .NotEmpty().WithMessage("معرف المحاضر مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف المحاضر غير صالح.");
        RuleFor(x => x.PriceInCredits).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationInMinutes).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
    }
}