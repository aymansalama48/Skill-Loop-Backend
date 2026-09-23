namespace Skill_Loop.Api.Contracts.Auth;

public sealed record LogoutRequest(
    string RefreshToken);
