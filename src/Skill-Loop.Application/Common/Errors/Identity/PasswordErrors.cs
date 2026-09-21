using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Identity;


/// <summary>
/// أخطاء عمليات كلمة المرور (تغيير / إعادة تعيين).
/// </summary>
public static class PasswordErrors
{
    public static readonly Error UserNotFound = new(
        "PASSWORD_USER_NOT_FOUND",
        "المستخدم غير موجود.",
        ErrorType.NotFound);

    public static readonly Error IncorrectCurrentPassword = new(
        "PASSWORD_INCORRECT_CURRENT",
        "كلمة المرور الحالية غير صحيحة.",
        ErrorType.Validation);

    public static readonly Error ChangeFailed = new(
        "PASSWORD_CHANGE_FAILED",
        "فشل تغيير كلمة المرور، تأكد أن الكلمة الجديدة تحقق الشروط المطلوبة.",
        ErrorType.Validation);

    public static readonly Error ResetFailed = new(
        "PASSWORD_RESET_FAILED",
        "فشلت إعادة تعيين كلمة المرور، الرابط غير صالح أو منتهي الصلاحية.",
        ErrorType.Validation);
}