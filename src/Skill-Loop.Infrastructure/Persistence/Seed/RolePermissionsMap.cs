using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Infrastructure.Persistence.Seed;

/// <summary>
/// خرائط الأدوار إلى الصلاحيات الممنوحة لها.
/// الـ ContextSeed بيستخدم الخريطة دي لربط كل دور بصلاحياته تلقائيًا.
/// </summary>
public static class RolePermissionsMap
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Map =
        new Dictionary<string, IReadOnlyList<string>>
        {
            // SuperAdmin: كل الصلاحيات (نستخدم GetAllPermissions عشان أي صلاحية جديدة تروح له تلقائيًا)
            [Roles.SuperAdmin] = Permissions.GetAllPermissions(),

            // Admin: إدارة المحتوى + المستخدمين + عرض الحجوزات
            [Roles.Admin] = new[]
            {
                Permissions.Catalog.CategoriesManage,
                Permissions.Catalog.SkillsManage,
                Permissions.Catalog.TagsManage,
                Permissions.Catalog.SessionsModerate,
                Permissions.Users.View,
                Permissions.Users.Activate,
                Permissions.Users.Deactivate,
                Permissions.Users.AssignRole,
                Permissions.Bookings.View,
            },

            // FinanceManager: كل ما يخص الفلوس والباقات
            [Roles.FinanceManager] = new[]
            {
                Permissions.Finance.PackagesManage,
                Permissions.Finance.PromoCodesManage,
                Permissions.Finance.WalletAdjust,
                Permissions.Finance.PaymentsView,
            },

            // Support: قراءة فقط
            [Roles.Support] = new[]
            {
                Permissions.Users.View,
                Permissions.Bookings.View,
            },

            // Roles.User و Roles.Instructor مش محتاجين صلاحيات Staff.
            // الحماية بتاعتهم بتتعمل بـ [Authorize] + Ownership Checks جوه الـ Handlers.
        };
}