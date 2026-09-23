namespace Skill_Loop.Api.Contracts.Profile;

public sealed record UpdateMyAccountProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);
