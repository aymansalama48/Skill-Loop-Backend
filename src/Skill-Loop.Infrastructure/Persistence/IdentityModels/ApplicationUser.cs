using Microsoft.AspNetCore.Identity;
using System.Numerics;

namespace Skill_Loop.Infrastructure.Persistence.IdentityModels;

public class ApplicationUser : IdentityUser<Guid>
{
    // البيانات الشخصية الأساسية
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// يعيد الاسم الكامل مع معالجة حقل الاسم الأوسط الاختياري بدون مسافات إضافية.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// رابط صورة البروفايل الشخصية (اختياري).
    /// </summary>
    public string? AvatarUrl { get; set; }

    // حالة الحساب
    public bool IsActive { get; set; } = true;

    // بيانات التتبع وتاريخ الدخول
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

}
