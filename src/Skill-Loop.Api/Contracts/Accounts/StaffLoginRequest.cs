namespace Skill_Loop.Api.Contracts.Accounts;

public sealed record LoginRequest(
    string Email,
    string Password);
