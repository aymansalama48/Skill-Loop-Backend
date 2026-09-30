using FluentValidation;

namespace Skill_Loop.Application.Common.Validation;

/// <summary>
/// Single source of truth for what an OTP code may look like.
///
/// Both validators previously hard-coded <c>Length(4)</c> while the generator produced
/// <c>OtpSettings:CodeLength</c> = 6 digits, so every genuinely valid code was rejected
/// with "Invalid value." That silently broke email verification and password reset - a
/// student could never confirm an address, and therefore could never log in.
///
/// The range below mirrors the clamp in <c>OtpService.GenerateSecureCode</c>
/// (<c>Math.Clamp(codeLength, 4, 10)</c>). Keep the two in step: this file is what the
/// Application layer can see, and the hash comparison inside <c>OtpService</c> remains the
/// authoritative check - these rules only reject input that could never be a real code.
/// </summary>
public static class OtpCodeValidationExtensions
{
    public const int MinLength = 4;
    public const int MaxLength = 10;

    public static IRuleBuilderOptions<T, string> ApplyOtpCodeRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("This field is required.")
            .Length(MinLength, MaxLength).WithMessage("Invalid value.")
            .Matches("^[0-9]+$").WithMessage("Invalid value.");
    }
}
