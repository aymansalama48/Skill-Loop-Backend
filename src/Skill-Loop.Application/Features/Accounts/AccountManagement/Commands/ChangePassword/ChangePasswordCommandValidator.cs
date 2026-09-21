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
            .NotEmpty().WithMessage("كلمة المرور الحالية مطلوبة.");

        // 2. التحقق من كلمة المرور الجديدة باستخدام القواعد الموحدة
        RuleFor(x => x.NewPassword)
            .ApplyStandardPasswordRules()
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("يجب أن تكون كلمة المرور الجديدة مختلفة عن كلمة المرور الحالية.");

        // 3. التحقق من تأكيد كلمة المرور الجديدة
        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("تأكيد كلمة المرور الجديدة مطلوب.")
            .Equal(x => x.NewPassword)
            .WithMessage("كلمة المرور الجديدة وتأكيدها غير متطابقين.");
    }
}