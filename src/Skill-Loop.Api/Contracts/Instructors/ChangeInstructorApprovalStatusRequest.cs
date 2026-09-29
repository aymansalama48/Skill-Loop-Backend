namespace Skill_Loop.Api.Contracts.Instructors;

public sealed record ChangeInstructorApprovalStatusRequest(
    bool IsApproved
);