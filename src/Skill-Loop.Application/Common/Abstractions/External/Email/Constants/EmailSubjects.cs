namespace Skill_Loop.Application.Common.Abstractions.External.Email.Constants;

/// <summary>
/// عناوين رسائل الإيميل (Subject Lines).
/// كل دالة بتستقبل اسم الشركة/التطبيق وترجّع العنوان النهائي كاملاً.
/// ملاحظة: العنوان النهائي بيتحط مباشرة في الـ EmailRequest.Subject،
/// فمفيش داعي لإضافة اسم التطبيق في الـ Service تاني.
/// </summary>
public static class EmailSubjects
{
    // ============================================================
    // Identity / Account
    // ============================================================

    public static string Login(string companyName)
        => $"إشعار تسجيل دخول جديد - {companyName}";

    public static string EmailConfirmation(string companyName)
        => $"تأكيد بريدك الإلكتروني - {companyName}";

    public static string ResetPassword(string companyName)
        => $"إعادة تعيين كلمة المرور - {companyName}";

    public static string Welcome(string companyName)
        => $"مرحباً بك في {companyName}";

    public static string PasswordChanged(string companyName)
        => $"تم تغيير كلمة المرور - {companyName}";

    public static string AccountLocked(string companyName)
        => $"تم قفل حسابك - {companyName}";

    public static string AccountUnlocked(string companyName)
        => $"تم فتح قفل حسابك - {companyName}";

    // ============================================================
    // Roles & Invitations
    // ============================================================

    public static string RoleAssigned(string companyName)
        => $"تم تعيين دور جديد لك - {companyName}";

    public static string RoleRemoved(string companyName)
        => $"تم إزالة دورك - {companyName}";

    public static string StaffInvitation(string companyName)
        => $"دعوة للانضمام إلى فريق العمل - {companyName}";

    // ============================================================
    // Sessions / Materials
    // ============================================================

    /// <summary>
    /// إشعار برفع مادة جديدة — بيتضمن اسم المادة اللي اترفعت.
    /// </summary>
    public static string MaterialUploaded(string materialName, string companyName)
        => $"تم رفع مادة جديدة: {materialName} - {companyName}";

    /// <summary>
    /// تأكيد رفع مادة — بيتضمن اسم المادة للتأكيد.
    /// </summary>
    public static string MaterialUploadedConfirmation(string materialName, string companyName)
        => $"تأكيد رفع مادة: {materialName} - {companyName}";

    // ============================================================
    // System / Admin Alerts
    // ============================================================

    /// <summary>
    /// تنبيه إداري عند اقتراب حدود تخزين Google Drive.
    /// </summary>
    public static string StorageQuotaWarning(string companyName)
        => $"تنبيه: اقتراب حدود تخزين Google Drive - {companyName}";
}