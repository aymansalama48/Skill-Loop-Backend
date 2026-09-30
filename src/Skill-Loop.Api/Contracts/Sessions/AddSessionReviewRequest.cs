namespace Skill_Loop.Api.Contracts.Sessions;

public sealed record AddSessionReviewRequest(
    int Stars,
    string? Comment);
