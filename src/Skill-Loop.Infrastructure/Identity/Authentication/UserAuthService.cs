using Microsoft.AspNetCore.Identity;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Authentication;
using Skill_Loop.Application.Common.Abstractions.Identity.Providers;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Features.Accounts.UserAuth.Shared;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Constants;
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
    // ???? ???? ???? UserAuthResponse
    public async Task<Result<UserAuthResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        // 1. ???? ???????? — ?? ?? ????? ?????? ??? ????? ???????? ?????
        //    (??? Email Enumeration)
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<UserAuthResponse>.Failure(UserErrors.InvalidCredentials);

        // 2. ?? ???? ???????? ????? (???? pre-sign-in checks)
        //    ?? ???? ????? ??? Email Enumeration
        var passwordValid = await userManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
        {
            // ????? ???????? ??????? (Lockout ??? ?????????)
            await userManager.AccessFailedAsync(user);

            // ?? ?????? ????? ???? ????????? ??????
            if (await userManager.IsLockedOutAsync(user))
                return Result<UserAuthResponse>.Failure(UserErrors.AccountLocked);

            // ????? ???? (????? ???????? ????? ???????? ?????)
            return Result<UserAuthResponse>.Failure(UserErrors.InvalidCredentials);
        }

        // 3. ? ???????? ?? — ????? ??? ?????????
        await userManager.ResetAccessFailedCountAsync(user);

        // 4. ?????? ?? ???? ???? ?????? (???????? ??? ???????? ????? ????? ????? ????? ???????)

        // 4.a — ??????? ?????
        if (!user.EmailConfirmed)
            return Result<UserAuthResponse>.Failure(UserErrors.EmailNotConfirmed);

        // 4.b — ?????? ????
        if (!user.IsActive)
            return Result<UserAuthResponse>.Failure(UserErrors.AccountDeactivated);

        // 4.c — ?? Staff ???? ?? ????? ??? User?
        var roles = await userManager.GetRolesAsync(user);
        if (roles.Any(r =>
                r == Roles.Admin ||
                r == Roles.SuperAdmin ||
                r == Roles.FinanceManager ||
                r == Roles.Support))
        {
            return Result<UserAuthResponse>.Failure(UserErrors.UseStaffPortal);
        }

        // 5. ????? ????????
        var accessToken = jwtTokenGenerator.GenerateJwtToken(
            user.Id, user.Email!, user.FullName, roles, []);
        var refreshToken = await refreshTokenService
            .GenerateAndSaveRefreshTokenAsync(user.Id, cancellationToken);

        user.LastLoginAt = dateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return Result<UserAuthResponse>.Success(new UserAuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresInSeconds = 3600,
            LoggedInAt = dateTime.UtcNow,
            Roles = roles.ToList()
        });
    }
    public async Task<Result<UserAuthResponse>> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var provider = externalAuthProviders.FirstOrDefault(p => p.ProviderName == "Google");
        if (provider is null) return Result<UserAuthResponse>.Failure(ExternalAuthErrors.InvalidToken);

        var tokenResult = await provider.ValidateTokenAsync(idToken, cancellationToken);
        if (!tokenResult.IsSuccess) return Result<UserAuthResponse>.Failure(tokenResult.Errors);

        var user = await userManager.FindByEmailAsync(tokenResult.Data!.Email); // ???????? Data ??? Value
        if (user is null) return Result<UserAuthResponse>.Failure(UserErrors.NotFound);

        // ??? ?????? ??????? ?? LoginAsync — ????? ???? ????? ?????? ????????
        if (!user.EmailConfirmed) return Result<UserAuthResponse>.Failure(UserErrors.EmailNotConfirmed);

        if (!user.IsActive) return Result<UserAuthResponse>.Failure(UserErrors.AccountDeactivated);

        var roles = await userManager.GetRolesAsync(user);

        // ?? Staff ???? ?? ????? ??? User?
        if (roles.Any(r =>
                r == Roles.Admin ||
                r == Roles.SuperAdmin ||
                r == Roles.FinanceManager ||
                r == Roles.Support))
        {
            return Result<UserAuthResponse>.Failure(UserErrors.UseStaffPortal);
        }

        var accessToken = jwtTokenGenerator.GenerateJwtToken(user.Id, user.Email!, user.FullName, roles, []);
        var refreshToken = await refreshTokenService.GenerateAndSaveRefreshTokenAsync(user.Id, cancellationToken);

        user.LastLoginAt = dateTime.UtcNow;
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