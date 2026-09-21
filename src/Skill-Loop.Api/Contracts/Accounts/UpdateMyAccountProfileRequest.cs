namespace Skill_Loop.Api.Contracts.Accounts;

public sealed record UpdateMyAccountProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);
