using FluentValidation;
using Skill_Loop.Application.Common.Validation;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ChangePassword;

/// <summary>
/// التحقق من صحة وقواعد بيانات تغيير كلمة المرور
/// </summary>
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        // 1. التحقق من كلمة المرور الحالية
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("This field is required.");

        // 2. التحقق من كلمة المرور الجديدة باستخدام القواعد الموحدة
        RuleFor(x => x.NewPassword)
            .ApplyStandardPasswordRules()
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Invalid value.");

        // 3. التحقق من تأكيد كلمة المرور الجديدة
        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("This field is required.")
            .Equal(x => x.NewPassword)
            .WithMessage("Invalid value.");
    }
}