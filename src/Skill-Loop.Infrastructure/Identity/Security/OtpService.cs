using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.OtpVerification;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Infrastructure.Options;
using Skill_Loop.Infrastructure.Persistence.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Skill_Loop.Infrastructure.Identity.Security;

/// <summary>
/// تنفيذ خدمة OTP — توليد وتحقق وإعادة إرسال أكواد التحقق برقم الموبايل
/// </summary>
public class OtpService(
    AppDbContext context,
    IOptions<OtpOptions> options,
    IDateTime dateTime) : IOtpService
{
    /// <summary>
    /// توليد OTP جديد — بيلغي أي كود سابق شغال لنفس الرقم/الغرض ويحفظ الكود بشكل مشفر
    /// </summary>
    public async Task<Result<OtpGenerationResult>> GenerateOtpAsync(
        string Identifier,
        OtpPurpose purpose,
        CancellationToken cancellationToken)
    {
        var nowUtc = dateTime.UtcNow;

        var oldActiveOtps = await context.OtpVerifications
            .Where(o => o.Identifier == Identifier
                     && o.Purpose == purpose
                     && !o.IsConsumed
                     && o.Expiry > nowUtc)
            .ToListAsync(cancellationToken);

        foreach (var old in oldActiveOtps)
            old.IsConsumed = true;

        // توليد كود بالطول المحدد في الإعدادات (OtpSettings:CodeLength)
        var code = GenerateSecureCode(options.Value.CodeLength);

        var otp = new OtpVerification
        {
            Id = Guid.CreateVersion7(),
            Identifier = Identifier,
            Purpose = purpose,
            CodeHash = HashCode(code, Identifier, purpose),
            Expiry = nowUtc.Add(options.Value.Expiry),
            NextResendAllowedAtUtc = nowUtc.Add(options.Value.ResendCooldown),
            AttemptsCount = 0,
            MaxAttempts = options.Value.MaxAttempts,
            IsConsumed = false
        };

        context.OtpVerifications.Add(otp);
        await context.SaveChangesAsync(cancellationToken);

        return Result<OtpGenerationResult>.Success(new OtpGenerationResult
        {
            Code = code,
            OtpId = otp.Id,
            ExpiresAtUtc = otp.Expiry,
            NextResendAllowedAtUtc = otp.NextResendAllowedAtUtc
        });
    }

    /// <summary>
    /// التحقق من الـ OTP — مع عدد محاولات محدود ومقارنة آمنة ضد الـ Timing Attacks
    /// </summary>
    public async Task<Result> ValidateOtpAsync(
        string Identifier,
        string code,
        OtpPurpose purpose,
        CancellationToken cancellationToken)
    {
        var otp = await context.OtpVerifications
            .Where(o => o.Identifier == Identifier && o.Purpose == purpose && !o.IsConsumed)
            .OrderByDescending(o => o.Expiry)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null)
            return Result.Failure(OtpErrors.NotFound);

        // Expiry and NextResendAllowedAtUtc are both stored in UTC, so they must be
        // compared against UtcNow. Comparing against the display-local clock shifts every
        // window by the UTC offset — the resend cooldown below became a two-hour block
        // instead of its configured two minutes.
        var nowUtc = dateTime.UtcNow;

        if (otp.Expiry < nowUtc)
            return Result.Failure(OtpErrors.Expired);

        // إذا كان الكود قد تجاوز المحاولات مسبقاً
        if (otp.AttemptsCount >= otp.MaxAttempts)
        {
            otp.IsConsumed = true;
            await context.SaveChangesAsync(cancellationToken);
            return Result.Failure(OtpErrors.MaxAttemptsExceeded);
        }

        var expectedHash = HashCode(code, Identifier, purpose);
        var isMatch = CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(expectedHash),
            Convert.FromBase64String(otp.CodeHash));

        if (!isMatch)
        {
            // تصحيح: زيادة عدد المحاولات ثم التحقق من تخطي الحد الأقصى
            otp.AttemptsCount++;

            if (otp.AttemptsCount >= otp.MaxAttempts)
                otp.IsConsumed = true; // إغلاق الكود إذا وصل للحد الأقصى الآن

            await context.SaveChangesAsync(cancellationToken);

            return otp.IsConsumed
                ? Result.Failure(OtpErrors.MaxAttemptsExceeded)
                : Result.Failure(OtpErrors.InvalidCode);
        }

        otp.IsConsumed = true;
        otp.VerifiedAt = nowUtc;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// إعادة إرسال OTP — مع احترام فترة الـ Cooldown
    /// </summary>
    public async Task<Result<OtpGenerationResult>> ResendOtpAsync(
        string Identifier,
        OtpPurpose purpose,
        CancellationToken cancellationToken)
    {
        var lastOtp = await context.OtpVerifications
            .Where(o => o.Identifier == Identifier && o.Purpose == purpose)
            .OrderByDescending(o => o.Expiry)
            .FirstOrDefaultAsync(cancellationToken);

        // NextResendAllowedAtUtc is persisted in UTC and must be compared against UtcNow.
        // Using the display-local clock here (UTC+2) blocked resend for two hours
        // regardless of the configured cooldown, which is a denial-of-service against the
        // legitimate user trying to receive their own code.
        if (lastOtp is not null && lastOtp.NextResendAllowedAtUtc > dateTime.UtcNow)
            return Result<OtpGenerationResult>.Failure(OtpErrors.ResendTooSoon);

        return await GenerateOtpAsync(Identifier, purpose, cancellationToken);
    }

    /// <summary>
    /// HMAC over the code, bound to both the identifier and the purpose.
    ///
    /// Binding the purpose means a code issued for email confirmation cannot validate
    /// against a password-reset row even if the two codes happen to collide. The row lookup
    /// is already scoped by purpose, so this is defence in depth — but key separation is
    /// cheap here, and a 6-digit code space makes collisions more plausible than they look.
    /// </summary>
    private string HashCode(string code, string Identifier, OtpPurpose purpose)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.Value.HashingSecret));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{purpose}:{Identifier}:{code}"));
        return Convert.ToBase64String(bytes);
    }

    // توليد كود أرقام آمن كريبتوجرافياً بالطول المطلوب في الإعدادات
    private static string GenerateSecureCode(int codeLength)
    {
        // Guard the configured value so a misconfigured section cannot collapse the key space.
        // The bounds come from OtpCodeValidationExtensions so the generator and the
        // validators can never disagree about what a legal code length is.
        var length = Math.Clamp(codeLength,
            Skill_Loop.Application.Common.Validation.OtpCodeValidationExtensions.MinLength,
            Skill_Loop.Application.Common.Validation.OtpCodeValidationExtensions.MaxLength);

        // RandomNumberGenerator.GetInt32 is exclusive on the upper bound, hence 10^length.
        var max = (int)Math.Pow(10, length);

        return RandomNumberGenerator.GetInt32(0, max).ToString($"D{length}");
    }
}