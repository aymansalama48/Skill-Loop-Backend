using FluentAssertions;
using Skill_Loop.Application.Common.Validation;
using Skill_Loop.Application.Features.Accounts.AccountManagement.Commands.ResetPassword;
using Skill_Loop.Application.Features.Accounts.UserAuth.Commands.VerifyEmailOtp;

namespace Skill_Loop.UnitTests.Features.Accounts.AccountManagement.Commands.ResetPassword;

/// <summary>
/// Regression tests for the OTP length mismatch.
///
/// Both validators hard-coded <c>Length(4)</c> while <c>OtpSettings:CodeLength</c> is 6 and
/// <c>OtpService.GenerateSecureCode</c> emits 6 digits. Every real code was therefore
/// rejected with "Invalid value.", which broke email verification and password reset
/// outright: a student could never confirm an address and so could never log in.
///
/// These assert the validators accept whatever length the generator can emit, rather than
/// pinning one number - the point is to fail if the two ever drift apart again.
/// </summary>
public class OtpCodeValidationTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]   // the configured default
    [InlineData(7)]
    [InlineData(10)]  // the upper clamp in GenerateSecureCode
    public void VerifyEmailOtp_AcceptsAnyLengthTheGeneratorCanProduce(int length)
    {
        var code = new string('1', length);

        var result = new VerifyEmailOtpCommandValidator().Validate(
            new VerifyEmailOtpCommand("user@test.com", code));

        result.IsValid.Should().BeTrue($"a {length}-digit code is one the generator can emit");
    }

    [Theory]
    [InlineData("123")]        // below the clamp
    [InlineData("12345678901")] // above the clamp
    [InlineData("")]           // empty
    [InlineData("12345a")]     // not all digits
    public void VerifyEmailOtp_RejectsImpossibleCodes(string code)
    {
        var result = new VerifyEmailOtpCommandValidator().Validate(
            new VerifyEmailOtpCommand("user@test.com", code));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(10)]
    public void ResetPassword_AcceptsAnyLengthTheGeneratorCanProduce(int length)
    {
        var command = new ResetPasswordCommand(
            "user@test.com",
            new string('2', length),
            "NewPassword@123",
            "NewPassword@123");

        var result = new ResetPasswordCommandValidator().Validate(command);

        result.IsValid.Should().BeTrue($"a {length}-digit code is one the generator can emit");
    }

    [Fact]
    public void PolicyBounds_MatchTheGeneratorClamp()
    {
        OtpCodeValidationExtensions.MinLength.Should().Be(4);
        OtpCodeValidationExtensions.MaxLength.Should().Be(10);
    }
}
