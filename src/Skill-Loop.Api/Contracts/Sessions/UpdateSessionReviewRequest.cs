namespace Skill_Loop.Api.Contracts.Sessions;

public sealed record UpdateSessionReviewRequest(
    int Stars,
    string? Comment);
