using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Identity;


/// <summary>
/// أخطاء تسجيل الدخول عبر مزوّدي الخدمة الخارجيين (Google, Facebook, ...).
/// </summary>
public static class ExternalAuthErrors
{
    public static readonly Error InvalidToken = new(
        "EXTERNAL_AUTH_INVALID_TOKEN",
        "توكن تسجيل الدخول غير صالح أو منتهي الصلاحية.",
        ErrorType.Validation);

    public static readonly Error EmailMissing = new(
        "EXTERNAL_AUTH_EMAIL_MISSING",
        "لا يمكن تسجيل الدخول بدون بريد إلكتروني من مزود الخدمة.",
        ErrorType.Validation);

    public static readonly Error EmailNotVerified = new(
        "EXTERNAL_AUTH_EMAIL_NOT_VERIFIED",
        "البريد الإلكتروني غير موثّق من مزود الخدمة.",
        ErrorType.Validation);
}