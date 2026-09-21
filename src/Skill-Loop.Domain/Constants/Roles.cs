using System.Numerics;

namespace Skill_Loop.Domain.Constants;

public static class Roles
{
    /// <summary>
    /// المدير العام - لديه كافة الصلاحيات على النظام
    /// </summary>
    public const string Admin = "Admin";


    /// <summary>
    /// قائمة بكل الأدوار المتاحة في النظام (للتكرار أو الـ Seed)
    /// </summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        Admin,

    };

}
