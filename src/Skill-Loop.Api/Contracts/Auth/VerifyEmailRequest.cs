namespace Skill_Loop.Api.Contracts.Auth
{
    public record VerifyEmailRequest(string Email, string OtpCode);
}
