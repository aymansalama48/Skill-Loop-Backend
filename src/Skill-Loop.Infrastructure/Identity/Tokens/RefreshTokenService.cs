using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.Tokens;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Features.Accounts.StaffAuth.Shared;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Identity.Tokens;
using Microsoft.EntityFrameworkCore;
/// <summary>
/// تنفيذ خدمة Refresh Token الخاصة بالموظفين (Staff)
///
/// Security properties maintained here:
/// 1. Tokens are stored only as a SHA-256 digest, never in plaintext.
/// 2. Lookup hashes the presented token and compares in constant time.
/// 3. Rotation links each token to a family, so replay of a rotated token revokes the
///    whole family instead of silently minting a new session.
/// </summary>
public class RefreshTokenService(
    AppDbContext context,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IPermissionService permissionService,   // 👈 مضافة
    IDateTime dateTime,
    ILogger<RefreshTokenService> logger) : IRefreshTokenService
{
    private static readonly TimeSpan RefreshTokenExpiry = TimeSpan.FromDays(7);

    /// <summary>
    /// توليد Refresh Token آمن وحفظه في قاعدة البيانات
    /// </summary>
    public async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var token = GenerateSecureToken();

        await SaveTokenAsync(userId, token, Guid.NewGuid(), cancellationToken: cancellationToken);

        logger.LogInformation("تم إنشاء Refresh Token جديد للمستخدم {UserId}", userId);

        return token;
    }

    /// <summary>
    /// Continues an existing token family, so a stolen-then-rotated token can be traced back
    /// to the original login and revoke all of its descendants.
    /// </summary>
    public async Task<string> GenerateAndSaveRefreshTokenAsync(
        Guid userId,
        Guid tokenFamilyId,
        CancellationToken cancellationToken)
    {
        var token = GenerateSecureToken();
        await SaveTokenAsync(userId, token, tokenFamilyId, cancellationToken);
        return token;
    }

    private async Task SaveTokenAsync(
        Guid userId,
        string plaintextToken,
        Guid tokenFamilyId,
        CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TokenHash = HashToken(plaintextToken),   // plaintext is never persisted
            TokenFamilyId = tokenFamilyId,
            ExpiryDate = dateTime.UtcNow.Add(RefreshTokenExpiry),
            CreatedAt = dateTime.UtcNow,
            IsRevoked = false
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// تبديل الـ Refresh Token — بيلغي القديم ويرجع Access Token + Refresh Token جديدين
    /// </summary>
    public async Task<Result<StaffAuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var storedToken = await FindByPlaintextAsync(refreshToken, cancellationToken);

        if (storedToken is null)
            return Result<StaffAuthResponse>.Failure(TokenErrors.InvalidRefreshToken);

        // Reuse of an already-rotated token: the token leaked, so the family is burned.
        if (storedToken.ReplacedByTokenHash is not null)
        {
            logger.LogWarning(
                "Refresh token reuse detected for user {UserId}; revoking the whole token family.",
                storedToken.UserId);

            await RevokeFamilyAsync(storedToken.TokenFamilyId, cancellationToken);
            return Result<StaffAuthResponse>.Failure(TokenErrors.TokenReuseDetected);
        }

        if (storedToken.IsRevoked)
            return Result<StaffAuthResponse>.Failure(TokenErrors.TokenRevoked);

        if (storedToken.ExpiryDate < dateTime.UtcNow)
            return Result<StaffAuthResponse>.Failure(TokenErrors.TokenExpired);

        var user = await userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null)
            return Result<StaffAuthResponse>.Failure(UserErrors.NotFound);

        // A deactivated account must not be able to refresh its way back in.
        if (!user.IsActive)
            return Result<StaffAuthResponse>.Failure(UserErrors.AccountDeactivated);

        if (!user.EmailConfirmed)
            return Result<StaffAuthResponse>.Failure(UserErrors.EmailNotConfirmed);

        var roles = await userManager.GetRolesAsync(user);

        // نفس مصدر الصلاحيات المستخدم في LoginAsync بالظبط
        var permissions = await permissionService.GetUserPermissionsAsync(user.Id, cancellationToken);

        var accessToken = jwtTokenGenerator.GenerateJwtToken(
            user.Id,
            user.Email!,
            user.FullName,
            roles,
            permissions
            );

        // Generate the successor first so the old row can point at it.
        var newPlaintext = GenerateSecureToken();
        var newHash = HashToken(newPlaintext);

        var successor = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            TokenHash = newHash,
            TokenFamilyId = storedToken.TokenFamilyId,
            ExpiryDate = dateTime.UtcNow.Add(RefreshTokenExpiry),
            CreatedAt = dateTime.UtcNow,
            IsRevoked = false
        };
        context.RefreshTokens.Add(successor);

        storedToken.IsRevoked = true;
        storedToken.LastUsedAt = dateTime.UtcNow;
        storedToken.ReplacedByTokenHash = newHash;

        await context.SaveChangesAsync(cancellationToken);

        return Result<StaffAuthResponse>.Success(new StaffAuthResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = newPlaintext,
            ExpiresInSeconds = 3600,
            Roles = roles.ToList(),
        });
    }

    /// <summary>
    /// إلغاء Refresh Token معين
    /// </summary>
    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var storedToken = await FindByPlaintextAsync(refreshToken, cancellationToken);

        if (storedToken is not null)
        {
            storedToken.IsRevoked = true;
            storedToken.LastUsedAt = dateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            // Never write the token value to logs.
            logger.LogInformation("تم إلغاء Refresh Token للمستخدم {UserId}", storedToken.UserId);
        }
    }

    /// <summary>
    /// إلغاء كل جلسات اليوزر (كل الأجهزة) — بتتستخدم عند تعطيل الحساب مثلاً
    /// </summary>
    public async Task<Result> RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        if (tokens.Count == 0)
            return Result.Success("لا توجد جلسات نشطة");

        foreach (var token in tokens)
            token.IsRevoked = true;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("تم إلغاء جميع Refresh Tokens للمستخدم {UserId} (عدد: {Count})", userId, tokens.Count);

        return Result.Success($"تم إلغاء {tokens.Count} جلسة");
    }

    /// <summary>
    /// Burns an entire token family. Used when a rotated token is replayed.
    /// </summary>
    private async Task RevokeFamilyAsync(Guid tokenFamilyId, CancellationToken cancellationToken)
    {
        var family = await context.RefreshTokens
            .Where(rt => rt.TokenFamilyId == tokenFamilyId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in family)
            token.IsRevoked = true;

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Resolves a client-presented token to its stored row by hashing it. The digest is
    /// compared in constant time, mirroring <c>OtpService</c>, so a partially-matching
    /// digest cannot be distinguished by response timing.
    /// </summary>
    private async Task<RefreshToken?> FindByPlaintextAsync(string plaintextToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(plaintextToken))
            return null;

        var presentedHash = HashToken(plaintextToken);

        var candidates = await context.RefreshTokens
            .Where(rt => rt.TokenHash == presentedHash)
            .ToListAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            if (CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(presentedHash),
                    Convert.FromBase64String(candidate.TokenHash)))
            {
                return candidate;
            }
        }

        return null;
    }

    // توليد توكن عشوائي آمن كريبتوجرافيًا
    private static string GenerateSecureToken()
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// One-way digest of a refresh token. SHA-256 (not a slow KDF) is correct here: the
    /// input is 256 bits of CSPRNG output, so there is no dictionary to brute-force, and a
    /// deliberate KDF would add latency to every refresh.
    /// </summary>
    private static string HashToken(string token) =>
        Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(token)));
}
