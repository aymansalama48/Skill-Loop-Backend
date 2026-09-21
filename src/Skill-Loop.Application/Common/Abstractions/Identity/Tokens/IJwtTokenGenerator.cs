namespace Skill_Loop.Application.Common.Abstractions.Identity.Tokens;

public interface IJwtTokenGenerator
{


    string GenerateJwtToken(
        Guid userId,
        string email,
        string fullName,
        IEnumerable<string> roles,
        IEnumerable<string> permissions);
}
