using Microsoft.AspNetCore.Identity;

namespace Skill_Loop.Infrastructure.Persistence.IdentityModels;

/// <summary>
/// يمثل الأدوار في النظام مع التوسع في خصائص ASP.NET Core Identity.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }

    public ApplicationRole(string roleName, string? description = null) : base(roleName)
    {
        Description = description;
    }

    // وصف تفصيلي للدور (مثل: "طبيب عيادة له صلاحيات الكشف والروشتات")
    public string? Description { get; set; }

    // يحدد ما إذا كان هذا الدور رئيسي في النظام ولا يمكن حذفه من الشاشة (مثل Admin)
    public bool IsSystemRole { get; set; } = false;

    // تاريخ إنشاء الدور
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property - العلاقة مع جدول الصلاحيات
    public ICollection<TbRolePermission> RolePermissions { get; set; } = new List<TbRolePermission>();
}