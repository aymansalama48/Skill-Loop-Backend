using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Authentication;

public interface IAuthService
{
    /// <summary>
    /// تسجيل الخروج المشترك (لكل من الـ Staff والـ User)
    /// </summary>
    Task<Result<bool>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}