namespace Skill_Loop.Api.Contracts.Instructors;

public sealed record CreateInstructorProfileRequest(
    string Headline,
    string Bio
);