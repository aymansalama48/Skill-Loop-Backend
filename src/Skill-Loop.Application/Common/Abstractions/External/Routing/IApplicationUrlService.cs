namespace Skill_Loop.Application.Common.Abstractions.External.Routing;

public interface IApplicationUrlService
{
    string GeneratePasswordResetUrl(
        string email,
        string token);
}
