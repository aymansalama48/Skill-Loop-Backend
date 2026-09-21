namespace Skill_Loop.Infrastructure.Persistence.IdentityModels;


/// <summary>
/// جدول الربط بين الأدوار والصلاحيات (Many-to-Many).
/// يحدد الصلاحيات الممنوحة لكل دور في النظام.
/// </summary>
public class TbRolePermission
{
    public Guid RoleId { get; set; }
    public ApplicationRole? Role { get; set; }
    public Guid PermissionId { get; set; }
    public TbPermission? Permission { get; set; }

    // تاريخ منح الصلاحية للدور
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}