using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Common.Abstractions.Identity.Security;

/// <summary>
/// مسؤول عن توليد OTP وتخزينه والتحقق منه فقط — لا يعرف شيئاً عن إرسال الرسائل (SMS)
/// </summary>
public interface IOtpService
{
    /// توليد OTP جديد وتخزينه (بدون إرسال — الإرسال مسؤولية خدمة تانية منفصلة)
    Task<Result<OtpGenerationResult>> GenerateOtpAsync(
        string phoneNumber,
        OtpPurpose purpose,
        CancellationToken cancellationToken);

    /// التحقق من صحة الكود مع خصم المحاولات المتبقية
    /// لاحظ: Result عادي مش Result<bool> — الفشل (كود غلط/منتهي/محاولات خلصت) بييجي في Error مش في Value
    Task<Result> ValidateOtpAsync(
        string phoneNumber,
        string code,
        OtpPurpose purpose,
        CancellationToken cancellationToken);

    /// إعادة إرسال باستخدام نفس الكود لو لسه صالح، أو توليد كود جديد لو خلص وقته
    Task<Result<OtpGenerationResult>> ResendOtpAsync(
        string phoneNumber,
        OtpPurpose purpose,
        CancellationToken cancellationToken);
}
public class OtpGenerationResult
{
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public Guid OtpId { get; set; }
    public DateTime NextResendAllowedAtUtc { get; set; }   // 👈 إضافة جديدة لمنع الـ SMS Bombing (كان ناقص قبل كده)
}