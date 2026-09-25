namespace Skill_Loop.Api.Contracts.Instructors;

public sealed record UpdateInstructorReviewRequest(
    int Rating,
    string? Comment
);