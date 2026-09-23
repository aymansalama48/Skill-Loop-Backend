namespace Skill_Loop.Api.Contracts.Profile;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
