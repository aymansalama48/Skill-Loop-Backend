namespace Skill_Loop.Api.Contracts.Auth;

public sealed record ResetPasswordRequest(
    string Email,
    string OtpCode,
    string NewPassword,
    string ConfirmPassword);
