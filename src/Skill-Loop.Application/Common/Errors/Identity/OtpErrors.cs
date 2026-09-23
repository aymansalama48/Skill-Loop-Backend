using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Identity;

/// <summary>
/// تجميع كافة أخطاء البزنس الخاصة بالـ OTP لسهولة الإدارة وتجنب النصوص المباشرة.
/// </summary>
public static class OtpErrors
{
    public static readonly Error NotFound = new(
        "OTP_NOT_FOUND",
        "لا يوجد كود تحقق صالح لهذا الايميل ، من فضلك اطلب كود جديد.",
        ErrorType.NotFound);

    public static readonly Error Expired = new(
        "OTP_EXPIRED",
        "انتهت صلاحية كود التحقق.",
        ErrorType.Validation);

    public static readonly Error InvalidCode = new(
        "OTP_INVALID_CODE",
        "كود التحقق غير صحيح.",
        ErrorType.Validation);

    public static readonly Error MaxAttemptsExceeded = new(
        "OTP_MAX_ATTEMPTS_EXCEEDED",
        "تجاوزت الحد الأقصى لعدد المحاولات، من فضلك اطلب كود جديد.",
        ErrorType.Validation);

    public static readonly Error ResendTooSoon = new(
        "OTP_RESEND_TOO_SOON",
        "من فضلك انتظر قليلاً قبل طلب كود جديد.",
        ErrorType.Validation);
}