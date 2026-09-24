using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;

public sealed class UpdateSessionCommandValidator : AbstractValidator<UpdateSessionCommand>
{
    public UpdateSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف الجلسة مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف الجلسة غير صالح.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان الجلسة مطلوب.")
            .MaximumLength(200).WithMessage("عنوان الجلسة لا يجب أن يتجاوز 200 حرف.");
    }
}