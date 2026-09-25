namespace Skill_Loop.Api.Contracts.Instructors;

public sealed record AddInstructorReviewRequest(
    int Rating,
    string? Comment
);