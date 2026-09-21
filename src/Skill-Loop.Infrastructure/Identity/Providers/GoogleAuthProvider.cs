using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.Identity.Providers;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.Identity.Providers;


/// <summary>
/// مزود الدخول بجوجل — بيتحقق من الـ Google ID Token ويرجع بيانات اليوزر
/// </summary>
public sealed class GoogleAuthProvider(
    IOptions<GoogleAuthOptions> options,
    ILogger<GoogleAuthProvider> logger) : IExternalAuthProvider
{
    /// <summary>
    /// اسم المزود (Google)
    /// </summary>
    public string ProviderName => "Google";

    /// <summary>
    /// التحقق من صلاحية توكن جوجل وإرجاع بيانات اليوزر الخارجي
    /// </summary>
    public async Task<Result<ExternalUserResult>> ValidateTokenAsync(
        string idToken,
        CancellationToken cancellationToken)   // 👈 مضافة، زي كل مكان تاني في المشروع
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings();

            if (!string.IsNullOrWhiteSpace(options.Value.ClientId))
                settings.Audience = new[] { options.Value.ClientId };

            // ملحوظة: المكتبة دي مالهاش Overload بياخد CancellationToken (قيد خارجي زي UserManager بالظبط)
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (string.IsNullOrWhiteSpace(payload.Email))
                return Result<ExternalUserResult>.Failure(ExternalAuthErrors.EmailMissing);

            if (!payload.EmailVerified)   // 👈 مهم أمنيًا — إيميل جوجل نفسه لازم يكون Verified
                return Result<ExternalUserResult>.Failure(ExternalAuthErrors.EmailNotVerified);

            return Result<ExternalUserResult>.Success(new ExternalUserResult(
                ProviderUserId: payload.Subject,
                Email: payload.Email.ToLowerInvariant(),
                FullName: payload.Name ?? payload.Email,
                AvatarUrl: payload.Picture));
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "توكن Google غير صالح");
            return Result<ExternalUserResult>.Failure(ExternalAuthErrors.InvalidToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطأ غير متوقع أثناء التحقق من توكن Google");
            return Result<ExternalUserResult>.Failure(ExternalAuthErrors.InvalidToken);
        }
    }
}