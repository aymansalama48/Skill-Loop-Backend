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
            .NotEmpty().WithMessage("This field is required.")
            .MinimumLength(8).WithMessage("Invalid value.")
            .Matches("[A-Z]").WithMessage("Invalid value.")
            .Matches("[a-z]").WithMessage("Invalid value.")
            .Matches("[0-9]").WithMessage("Invalid value.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Invalid value.");
    }
}