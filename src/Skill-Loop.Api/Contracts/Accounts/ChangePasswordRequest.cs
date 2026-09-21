namespace Skill_Loop.Api.Contracts.Accounts;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
