namespace Skill_Loop.Api.Contracts.Accounts
{
    public record RegisterUserRequest(string FirstName, string LastName, string Email, string Password);
}
