using Microsoft.AspNetCore.Identity;
using Skill_Loop.Application.Common.Abstractions.Identity.Authentication;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Identity.Authentication;

public class AuthService(
    IRefreshTokenService refreshTokenService,
    SignInManager<ApplicationUser> signInManager) : IAuthService
{
    public async Task<Result<bool>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // 1. إبطال التوكن من قاعدة البيانات
        await refreshTokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);

        // 2. مسح ملفات تعريف الارتباط (إن وجدت)
        await signInManager.SignOutAsync();

        return Result<bool>.Success(true);
    }
}