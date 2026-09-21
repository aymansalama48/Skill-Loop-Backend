using FluentValidation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.UpdateMyProfile;

public sealed class UpdateMyAccountProfileCommandValidator : AbstractValidator<UpdateMyAccountProfileCommand>
{
    public UpdateMyAccountProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("الاسم الأول مطلوب.")
            .MaximumLength(50).WithMessage("الاسم الأول لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("الاسم الأخير مطلوب.")
            .MaximumLength(50).WithMessage("الاسم الأخير لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("رقم الهاتف طويل جداً.");
    }
}