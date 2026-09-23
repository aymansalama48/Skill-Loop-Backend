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
        var oldActiveOtps = await context.OtpVerifications
            .Where(o => o.Identifier == Identifier
                     && o.Purpose == purpose
                     && !o.IsConsumed
                     && o.Expiry > dateTime.Now)
            .ToListAsync(cancellationToken);

        foreach (var old in oldActiveOtps)
            old.IsConsumed = true;

        // توليد كود من 4 أرقام فقط
        var code = GenerateSecure4DigitCode();
        var nowUtc = dateTime.Now; 

        var otp = new OtpVerification
        {
            Id = Guid.CreateVersion7(),
            Identifier = Identifier,
            Purpose = purpose,
            CodeHash = HashCode(code, Identifier),
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

        if (otp.Expiry < dateTime.Now) 
            return Result.Failure(OtpErrors.Expired);

        // إذا كان الكود قد تجاوز المحاولات مسبقاً
        if (otp.AttemptsCount >= otp.MaxAttempts)
        {
            otp.IsConsumed = true;
            await context.SaveChangesAsync(cancellationToken);
            return Result.Failure(OtpErrors.MaxAttemptsExceeded);
        }

        var expectedHash = HashCode(code, Identifier);
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
        otp.VerifiedAt = dateTime.Now;
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

        if (lastOtp is not null && lastOtp.NextResendAllowedAtUtc > dateTime.Now) 
            return Result<OtpGenerationResult>.Failure(OtpErrors.ResendTooSoon);

        return await GenerateOtpAsync(Identifier, purpose, cancellationToken);
    }

    private string HashCode(string code, string Identifier)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.Value.HashingSecret));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{Identifier}:{code}"));
        return Convert.ToBase64String(bytes);
    }

    // توليد كود 4 أرقام آمن كريبتوجرافياً
    private static string GenerateSecure4DigitCode()
    {
        // يولد رقم عشوائي بين 0 و 9999 
        // التنسيق "D4" يضمن أنه لو كان الرقم مثلاً 5، سيتحول إلى "0005"
        return RandomNumberGenerator.GetInt32(0, 10000).ToString("D4");
    }
}