namespace Skill_Loop.Domain.Constants;

public static class Roles
{
    /// <summary>
    /// المدير العام — لديه كافة الصلاحيات على النظام
    /// </summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>
    /// مدير المحتوى — التصنيفات والمهارات والجلسات وإدارة المستخدمين
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// المسؤول المالي — الباقات والبرومو والمحفظة والمدفوعات
    /// </summary>
    public const string FinanceManager = "FinanceManager";

    /// <summary>
    /// الدعم — عرض المستخدمين والحجوزات (قراءة فقط)
    /// </summary>
    public const string Support = "Support";

    /// <summary>
    /// المستخدم العادي (Learner) — تصفح، حجز، محفظة، شات، تقييم
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// المعلم — يُمنح عند تفعيل InstructorProfile
    /// </summary>
    public const string Instructor = "Instructor";

    /// <summary>
    /// قائمة بكل الأدوار المتاحة في النظام (للتكرار أو الـ Seed)
    /// </summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        SuperAdmin,
        Admin,
        FinanceManager,
        Support,
        User,
        Instructor
    };

    /// <summary>
    /// الأدوار الخاصة بالـ Staff (لوحة التحكم)
    /// </summary>
    public static readonly IReadOnlyList<string> StaffRoles = new[]
    {
        SuperAdmin,
        Admin,
        FinanceManager,
        Support
    };

    /// <summary>
    /// الأدوار الخاصة بمستخدمي الموبايل
    /// </summary>
    public static readonly IReadOnlyList<string> MobileRoles = new[]
    {
        User,
        Instructor
    };
}