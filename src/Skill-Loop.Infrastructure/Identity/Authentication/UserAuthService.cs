using Microsoft.AspNetCore.Identity;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Authentication;
using Skill_Loop.Application.Common.Abstractions.Identity.Providers;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Features.Accounts.UserAuth.Shared;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Identity.Authentication;

public class UserAuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenService refreshTokenService,
    IDateTime dateTime,
    IEnumerable<IExternalAuthProvider> externalAuthProviders) : IUserAuthService
{
    // لاحظ أننا نرجع UserAuthResponse
    public async Task<Result<UserAuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<UserAuthResponse>.Failure(UserErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result<UserAuthResponse>.Failure(UserErrors.AccountDeactivated);

        if (!user.EmailConfirmed)
            return Result<UserAuthResponse>.Failure(UserErrors.LoginNotAllowed);

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut) return Result<UserAuthResponse>.Failure(UserErrors.AccountLocked);
            return Result<UserAuthResponse>.Failure(UserErrors.InvalidCredentials);
        }

        var roles = await userManager.GetRolesAsync(user);

        var accessToken = jwtTokenGenerator.GenerateJwtToken(user.Id, user.Email!, user.FullName, roles, []);
        var refreshToken = await refreshTokenService.GenerateAndSaveRefreshTokenAsync(user.Id, cancellationToken);

        user.LastLoginAt = dateTime.Now;
        await userManager.UpdateAsync(user);

        return Result<UserAuthResponse>.Success(new UserAuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresInSeconds = 3600,
            LoggedInAt = dateTime.Now,
            Roles = roles.ToList()
        });
    }

    public async Task<Result<UserAuthResponse>> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var provider = externalAuthProviders.FirstOrDefault(p => p.ProviderName == "Google");
        if (provider is null) return Result<UserAuthResponse>.Failure(ExternalAuthErrors.InvalidToken);

        var tokenResult = await provider.ValidateTokenAsync(idToken, cancellationToken);
        if (!tokenResult.IsSuccess) return Result<UserAuthResponse>.Failure(tokenResult.Errors);

        var user = await userManager.FindByEmailAsync(tokenResult.Data!.Email); // استخدمنا Data بدل Value
        if (user is null) return Result<UserAuthResponse>.Failure(UserErrors.NotFound);
        if (!user.IsActive) return Result<UserAuthResponse>.Failure(UserErrors.AccountDeactivated);

        var roles = await userManager.GetRolesAsync(user);

        var accessToken = jwtTokenGenerator.GenerateJwtToken(user.Id, user.Email!, user.FullName, roles, []);
        var refreshToken = await refreshTokenService.GenerateAndSaveRefreshTokenAsync(user.Id, cancellationToken);

        user.LastLoginAt = dateTime.Now;
        await userManager.UpdateAsync(user);

        return Result<UserAuthResponse>.Success(new UserAuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresInSeconds = 3600,
            Roles = roles.ToList()
        });
    }
}