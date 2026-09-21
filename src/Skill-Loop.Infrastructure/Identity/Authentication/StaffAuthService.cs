using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Authentication;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.Providers;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Identity.Authentication;

/// <summary>
/// تنفيذ خدمة مصادقة الموظفين (Staff: Admin, Doctor, Receptionist)
/// </summary>
public class StaffAuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenService refreshTokenService,
    IPermissionService permissionService,   // 👈 مضافة بدل الـ Claims المباشرة
    IDateTime dateTime,
    AppDbContext context,
    IEnumerable<IExternalAuthProvider> externalAuthProviders,   // 👈 جديدة في الكونستركتور
    ILogger<StaffAuthService> logger) : IStaffAuthService
{
    /// <summary>
    /// تسجيل دخول الموظف بالإيميل والباسورد مع تطبيق الـ Lockout
    /// </summary>
    public async Task<Result<StaffAuthResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result<StaffAuthResponse>.Failure(UserErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result<StaffAuthResponse>.Failure(UserErrors.AccountDeactivated);

        // CheckPasswordSignInAsync بدل PasswordSignInAsync — عشان منعملش Cookie Sign-in
        // غير مطلوب في API قايم على JWT بس، مع الاحتفاظ بنفس فايدة الـ Lockout
        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
                return Result<StaffAuthResponse>.Failure(UserErrors.AccountLocked);
            if (signInResult.IsNotAllowed)
                return Result<StaffAuthResponse>.Failure(UserErrors.LoginNotAllowed);
            return Result<StaffAuthResponse>.Failure(UserErrors.InvalidCredentials);
        }

        var roles = await userManager.GetRolesAsync(user);

        // الصلاحيات دلوقتي بتيجي من IPermissionService (Role-based) بدل Claims مباشرة على اليوزر
        var permissions = await permissionService.GetUserPermissionsAsync(user.Id, cancellationToken);


        var accessToken = jwtTokenGenerator.GenerateJwtToken(
            user.Id,
            user.Email!,
            user.FullName,
            roles,
            permissions);

        var refreshToken = await refreshTokenService.GenerateAndSaveRefreshTokenAsync(user.Id, cancellationToken);

        user.LastLoginAt = dateTime.Now;
        await userManager.UpdateAsync(user);

        logger.LogInformation("تم تسجيل دخول الموظف {Email} بنجاح", email);

        return Result<StaffAuthResponse>.Success(new StaffAuthResponse
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

    /// <summary>
    /// تسجيل خروج الموظف — إلغاء الـ Refresh Token و Sign-out
    /// </summary>
    public async Task<Result<bool>> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        await refreshTokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
        await signInManager.SignOutAsync();

        logger.LogInformation("تم تسجيل خروج الموظف بنجاح (Refresh Token: {Token})", refreshToken);

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// دخول Staff بجوجل — لا ينشئ حساب جديد أبدًا، لازم يكون الحساب موجود بالفعل
    /// (اتعمل من قبل عن طريق نظام الدعوات) وإلا نظام الدعوات بيبقى بلا فايدة
    /// </summary>
    public async Task<Result<StaffAuthResponse>> LoginWithGoogleAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        var provider = externalAuthProviders.FirstOrDefault(p => p.ProviderName == "Google");
        if (provider is null)
            return Result<StaffAuthResponse>.Failure(ExternalAuthErrors.InvalidToken);

        var tokenResult = await provider.ValidateTokenAsync(idToken, cancellationToken);
        if (!tokenResult.IsSuccess)
            return Result<StaffAuthResponse>.Failure(tokenResult.Errors);

        var externalUser = tokenResult.Data!;

        var user = await userManager.FindByEmailAsync(externalUser.Email);
        if (user is null)
            return Result<StaffAuthResponse>.Failure(UserErrors.InvalidCredentials); // "محتاج دعوة من الأدمن الأول"

        if (!user.IsActive)
            return Result<StaffAuthResponse>.Failure(UserErrors.AccountDeactivated);

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Count == 0)
            return Result<StaffAuthResponse>.Failure(UserErrors.InvalidCredentials); // مش حساب Staff فعليًا

        var permissions = await permissionService.GetUserPermissionsAsync(user.Id, cancellationToken);

        var accessToken = jwtTokenGenerator.GenerateJwtToken(
            user.Id, user.Email!, user.FullName, roles, permissions);

        var refreshToken = await refreshTokenService.GenerateAndSaveRefreshTokenAsync(user.Id, cancellationToken);

        user.LastLoginAt = dateTime.Now;
        await userManager.UpdateAsync(user);

        logger.LogInformation("تم تسجيل دخول الموظف {Email} بجوجل", externalUser.Email);

        return Result<StaffAuthResponse>.Success(new StaffAuthResponse
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