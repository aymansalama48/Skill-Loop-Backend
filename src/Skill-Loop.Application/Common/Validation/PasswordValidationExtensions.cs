using FluentValidation;

namespace Skill_Loop.Application.Common.Validation;

public static class PasswordValidationExtensions
{
    /// <summary>
    /// يطبق نفس قواعد كلمة المرور الموجودة في إعدادات Identity
    /// RequireDigit, RequireLowercase, RequireUppercase, RequireNonAlphanumeric, RequiredLength = 8
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyStandardPasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل.")
            .Matches("[A-Z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل.")
            .Matches("[a-z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف صغير واحد على الأقل.")
            .Matches("[0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رقم واحد على الأقل.")
            .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رمز خاص (مثل @, #, $, !) واحد على الأقل.");
    }
}