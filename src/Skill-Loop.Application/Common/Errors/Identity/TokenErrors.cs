using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Identity;

/// <summary>
/// أخطاء خاصة بـ Refresh Token للموظفين.
/// </summary>
public static class TokenErrors
{
    /// <summary>
    /// Refresh Token غير صالح أو غير موجود.
    /// </summary>
    public static readonly Error InvalidRefreshToken = new(
        "TOKEN_INVALID_REFRESH_TOKEN",
        "Refresh Token غير صالح أو غير موجود.",
        ErrorType.Validation);

    /// <summary>
    /// Refresh Token ملغي (تم تسجيل الخروج منه).
    /// </summary>
    public static readonly Error TokenRevoked = new(
        "TOKEN_REVOKED",
        "Refresh Token ملغي، يرجى تسجيل الدخول مجدداً.",
        ErrorType.Validation);

    /// <summary>
    /// انتهت صلاحية Refresh Token.
    /// </summary>
    public static readonly Error TokenExpired = new(
        "TOKEN_EXPIRED",
        "انتهت صلاحية Refresh Token، يرجى تسجيل الدخول مجدداً.",
        ErrorType.Validation);

    /// <summary>
    /// Refresh Token كان قد تم تدويره (rotated) وتم استخدامه مرة أخرى.
    /// ده مؤشر على سرقة التوكن، لذلك يتم إلغاء كل التوكنات في نفس العائلة.
    /// </summary>
    public static readonly Error TokenReuseDetected = new(
        "TOKEN_REUSE_DETECTED",
        "تم إلغاء جميع الجلسات لأسباب أمنية. يرجى تسجيل الدخول مجدداً.",
        ErrorType.Forbidden);

    /// <summary>
    /// فشل في إنشاء Refresh Token بسبب خطأ غير متوقع.
    /// </summary>
    public static readonly Error TokenGenerationFailed = new(
        "TOKEN_GENERATION_FAILED",
        "حدث خطأ أثناء إنشاء Refresh Token.",
        ErrorType.Failure);
}