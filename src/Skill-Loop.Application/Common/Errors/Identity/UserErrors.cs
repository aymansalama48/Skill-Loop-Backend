using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Identity;

/// <summary>
/// أخطاء عامة متعلقة بحسابات المستخدمين (تخص جميع أنواع المستخدمين: Staff، Patient، Admin).
/// </summary>
public static class UserErrors
{
    // ----- أخطاء عامة -----
    public static readonly Error NotFound = new(
        "USER_NOT_FOUND",
        "المستخدم غير موجود.",
        ErrorType.NotFound);

    public static readonly Error InvalidCredentials = new(
        "USER_INVALID_CREDENTIALS",
        "البريد الإلكتروني أو كلمة المرور غير صحيحة.",
        ErrorType.Validation);

    public static readonly Error AccountLocked = new(
        "USER_ACCOUNT_LOCKED",
        "تم قفل الحساب بسبب كثرة المحاولات الفاشلة، يرجى المحاولة لاحقاً.",
        ErrorType.Validation);

    public static readonly Error AccountDeactivated = new(
        "USER_ACCOUNT_DEACTIVATED",
        "هذا الحساب غير نشط، يرجى التواصل مع الدعم الفني.",
        ErrorType.Validation);

    public static readonly Error EmailAlreadyExists = new(
        "USER_EMAIL_ALREADY_EXISTS",
        "البريد الإلكتروني مستخدم بالفعل في حساب آخر.",
        ErrorType.Validation);

    public static readonly Error PhoneAlreadyExists = new(
        "USER_PHONE_ALREADY_EXISTS",
        "رقم الهاتف مستخدم بالفعل في حساب آخر.",
        ErrorType.Validation);

    public static readonly Error LoginNotAllowed = new(
        "USER_LOGIN_NOT_ALLOWED",
        "تسجيل الدخول غير مسموح لهذا الحساب.",
        ErrorType.Validation);

    // ----- أخطاء ديناميكية -----
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