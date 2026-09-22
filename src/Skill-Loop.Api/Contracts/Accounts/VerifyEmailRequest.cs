namespace Skill_Loop.Api.Contracts.Accounts
{
    public record VerifyEmailRequest(string Email, string OtpCode);
}
