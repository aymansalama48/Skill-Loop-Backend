using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.OtpVerification;

/// <summary>
/// سجل مستقل للتحقق عبر OTP
/// </summary>
public class OtpVerification : BaseEntity
{
    // 👈 تم التعديل هنا: Identifier يقبل (رقم هاتف أو بريد إلكتروني)
    public string Identifier { get; set; } = string.Empty;

    /// الكود بيتخزن Hashed دايمًا (HMAC)، مش نص عادي
    public string CodeHash { get; set; } = string.Empty;

    public DateTime Expiry { get; set; }
    public DateTime NextResendAllowedAtUtc { get; set; }

    public OtpPurpose Purpose { get; set; }

    /// اتستهلك = نجح التحقق، أو اتقفل بسبب Max Attempts، أو اتلغى لصدور كود جديد بدله
    public bool IsConsumed { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }

    public int AttemptsCount { get; set; } = 0;
    public int MaxAttempts { get; set; } = 5;
    public bool IsMaxAttemptsReached => AttemptsCount >= MaxAttempts;
}