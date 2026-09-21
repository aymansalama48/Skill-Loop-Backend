namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetMyProfile;

public record MyAccountProfileResponse(
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl
);