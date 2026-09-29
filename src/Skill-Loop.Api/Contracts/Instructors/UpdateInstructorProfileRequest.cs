namespace Skill_Loop.Api.Contracts.Instructors;

public sealed record UpdateInstructorProfileRequest(
    string Headline,
    string Bio
);