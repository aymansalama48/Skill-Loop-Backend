
using FluentValidation;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffGoogleLogin;

namespace Skill_Loop.Application.Features.Accounts.StaffAuth.Commands.StaffGoogleLogin;
public sealed class StaffGoogleLoginCommandValidator : AbstractValidator<StaffGoogleLoginCommand>
{
    public StaffGoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("رمز Google (IdToken) مطلوب");
    }
}