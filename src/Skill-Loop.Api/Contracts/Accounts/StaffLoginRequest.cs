namespace Skill_Loop.Api.Contracts.Accounts;

public sealed record StaffLoginRequest(
    string Email,
    string Password);
