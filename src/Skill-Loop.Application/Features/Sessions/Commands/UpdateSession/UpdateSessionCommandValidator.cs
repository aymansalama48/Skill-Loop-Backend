using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;

public sealed class UpdateSessionCommandValidator : AbstractValidator<UpdateSessionCommand>
{
    public UpdateSessionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("معرف الجلسة مطلوب.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PriceInCredits).GreaterThanOrEqualTo(0).WithMessage("السعر يجب أن يكون صفراً أو أكثر.");
        RuleFor(x => x.DurationInMinutes).GreaterThan(0).WithMessage("مدة الجلسة يجب أن تكون محددة بالدقائق.");
        RuleFor(x => x.Type).IsInEnum().WithMessage("نوع الجلسة غير صالح.");
    }
}