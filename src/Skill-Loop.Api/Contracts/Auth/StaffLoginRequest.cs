namespace Skill_Loop.Api.Contracts.Auth;

public sealed record LoginRequest(
    string Email,
    string Password);
