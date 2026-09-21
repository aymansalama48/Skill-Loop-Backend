namespace Skill_Loop.Infrastructure.Persistence.IdentityModels;

/// <summary>
/// يمثل جدول الصلاحيات في النظام (Permissions Catalog).
/// يتم تعريف كل أكواد الصلاحيات المتاحة في التطبيق داخل هذا الجدول.
/// </summary>
public class TbPermission
{
    // المعرف الفريد للصلاحية
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// كود الصلاحية البرمجي الذي يتم الفحص عليه داخل الـ PermissionAttribute.
    /// مثال: "Patients.Create", "Appointments.Cancel"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// الاسم المعروض للصلاحية في واجهة الشاشة للآدمن.
    /// مثال: "إضافة مريض جديد", "إلغاء الموعد"
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// الموديول أو النظام الفرعي التابعة له الصلاحية لتسهيل التجميع في الـ UI.
    /// مثال: "Patients", "Appointments", "Billing", "Settings"
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// وصف تفصيلي لوظيفة الصلاحية (اختياري).
    /// </summary>
    public string? Description { get; set; }

    // تاريخ إنشاء الصلاحية في السيستم
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties - العلاقات
    // العلاقة مع جدول الربط بين الأدوار والصلاحيات (TbRolePermission)
    public ICollection<TbRolePermission> RolePermissions { get; set; } = new List<TbRolePermission>();
}
