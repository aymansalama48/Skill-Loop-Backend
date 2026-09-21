namespace Skill_Loop.Application.Common.Abstractions.External.Email.Constants;

public static class EmailSubjects
{
    public static string EmailConfirmation(string companyName) => $"تأكيد بريدك الإلكتروني - {companyName}";
    public static string ResetPassword(string companyName) => $"إعادة تعيين كلمة المرور - {companyName}";
    public static string Welcome(string companyName) => $"مرحباً بك في {companyName}";
    public static string PasswordChanged(string companyName) => $"تم تغيير كلمة المرور - {companyName}";
    public static string AccountLocked(string companyName) => $"تم قفل حسابك - {companyName}";
    public static string AccountUnlocked(string companyName) => $"تم فتح قفل حسابك - {companyName}";

}
