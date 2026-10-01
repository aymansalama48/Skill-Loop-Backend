namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetMyProfile;

public record MyAccountProfileResponse(
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    string? InstructorHeadline = null,
    string? InstructorBio = null,
    bool? IsInstructorApproved = null,
    double? InstructorRating = null,
    int? InstructorSessionsCompleted = null
);