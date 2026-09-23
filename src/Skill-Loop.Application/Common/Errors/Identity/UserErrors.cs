using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Identity;

/// <summary>
/// أخطاء عامة متعلقة بحسابات المستخدمين
/// </summary>
public static class UserErrors
{
    // ==========================================================
    // أخطاء المصادقة (Authentication) — ترجّع 401/403
    // ==========================================================

    /// <summary>
    /// بيانات الدخول غلط (إيميل غير موجود أو باسورد غلط).
    /// مقصود إن الرسالة عامة عشان ما نسرّبش لو الإيميل موجود.
    /// </summary>
    public static readonly Error InvalidCredentials = new(
        "USER_INVALID_CREDENTIALS",
        "البريد الإلكتروني أو كلمة المرور غير صحيحة. تأكد من البيانات وحاول تاني.",
        ErrorType.Unauthorized);   // 👈 401 مش 400

    /// <summary>
    /// الإيميل لسه ما اتأكدش. المستخدم لازم يروح لشاشة الـ OTP.
    /// </summary>
    public static readonly Error EmailNotConfirmed = new(
        "USER_EMAIL_NOT_CONFIRMED",
        "لم يتم تأكيد بريدك الإلكتروني بعد. من فضلك أدخل كود التحقق الذي أرسلناه لك.",
        ErrorType.Forbidden);   // 👈 403

    /// <summary>
    /// الحساب موقوف.
    /// </summary>
    public static readonly Error AccountDeactivated = new(
        "USER_ACCOUNT_DEACTIVATED",
        "حسابك موقوف حالياً. للاستفسار يرجى التواصل مع الدعم الفني.",
        ErrorType.Forbidden);   // 👈 403

    /// <summary>
    /// الحساب مقفول بسبب محاولات دخول فاشلة كتير.
    /// </summary>
    public static readonly Error AccountLocked = new(
        "USER_ACCOUNT_LOCKED",
        "تم قفل حسابك مؤقتاً بسبب كثرة المحاولات الفاشلة. حاول تاني بعد 15 دقيقة أو أعد تعيين كلمة المرور.",
        ErrorType.Forbidden);   // 👈 403

    /// <summary>
    /// مستخدم Staff بيحاول يدخل من بوابة المستخدم العادي.
    /// </summary>
    public static readonly Error UseStaffPortal = new(
        "USER_USE_STAFF_PORTAL",
        "هذا الحساب مخصص للوحة التحكم. الرجاء تسجيل الدخول من بوابة الـ Staff.",
        ErrorType.Forbidden);   // 👈 403

    // ==========================================================
    // أخطاء التسجيل (Registration) — ترجّع 409 Conflict
    // ==========================================================

    public static readonly Error EmailAlreadyExists = new(
        "USER_EMAIL_ALREADY_EXISTS",
        "البريد الإلكتروني مستخدم بالفعل في حساب آخر.",
        ErrorType.Conflict);   // 👈 409

    public static readonly Error PhoneAlreadyExists = new(
        "USER_PHONE_ALREADY_EXISTS",
        "رقم الهاتف مستخدم بالفعل في حساب آخر.",
        ErrorType.Conflict);   // 👈 409

    public static readonly Error EmailAlreadyConfirmed = new(
        "USER_EMAIL_ALREADY_CONFIRMED",
        "هذا الحساب مُفعّل بالفعل، يمكنك تسجيل الدخول مباشرة.",
        ErrorType.Conflict);   // 👈 409

    // ==========================================================
    // أخطاء عامة
    // ==========================================================

    public static readonly Error NotFound = new(
        "USER_NOT_FOUND",
        "المستخدم غير موجود.",
        ErrorType.NotFound);

    public static readonly Error LoginNotAllowed = new(
        "USER_LOGIN_NOT_ALLOWED",
        "تسجيل الدخول غير مسموح لهذا الحساب.",
        ErrorType.Forbidden);

    // ==========================================================
    // أخطاء ديناميكية
    // ==========================================================

    public static Error UpdateFailed(string details) => new(
        "USER_UPDATE_FAILED",
        $"فشل تحديث ملف المستخدم: {details}",
        ErrorType.Failure);

    public static Error CreationFailed(string details) => new(
        "USER_CREATION_FAILED",
        $"فشل إنشاء المستخدم: {details}",
        ErrorType.Failure);

    public static Error InvalidPasswordResetToken(string details) => new(
        "USER_INVALID_PASSWORD_RESET_TOKEN",
        $"رمز إعادة تعيين كلمة المرور غير صالح: {details}",
        ErrorType.Validation);

    public static Error ValidationFailed(string details) => new(
        "USER_VALIDATION_FAILED",
        $"فشل التحقق من البيانات: {details}",
        ErrorType.Validation);
}