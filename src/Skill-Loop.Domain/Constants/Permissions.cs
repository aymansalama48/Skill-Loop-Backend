using System.Reflection;

namespace Skill_Loop.Domain.Constants;

public class Permissions
{
    /// <summary>
    /// إدارة التصنيفات والمهارات والجلسات
    /// </summary>
    public static class Catalog
    {
        public const string CategoriesManage = "Categories.Manage";
        public const string SkillsManage = "Skills.Manage";
        public const string TagsManage = "Tags.Manage";
        public const string SessionsModerate = "Sessions.Moderate";
    }

    /// <summary>
    /// إدارة المستخدمين — تفعيل، إيقاف، تعيين أدوار
    /// </summary>
    public static class Users
    {
        public const string View = "Users.View";
        public const string Activate = "Users.Activate";
        public const string Deactivate = "Users.Deactivate";
        public const string AssignRole = "Users.AssignRole";
    }

    /// <summary>
    /// العمليات المالية — الباقات، البرومو، المحفظة، المدفوعات
    /// </summary>
    public static class Finance
    {
        public const string PackagesManage = "Packages.Manage";
        public const string PromoCodesManage = "PromoCodes.Manage";
        public const string WalletAdjust = "Wallet.Adjust";
        public const string PaymentsView = "Payments.View";
    }

    /// <summary>
    /// الحجوزات (قراءة فقط للدعم والإدارة)
    /// </summary>
    public static class Bookings
    {
        public const string View = "Bookings.View";
    }

    /// <summary>
    /// التحكم في الوصول — الأدوار والصلاحيات والدعوات
    /// </summary>
    public static class Access
    {
        public const string RolesManage = "Roles.Manage";
        public const string PermissionsManage = "Permissions.Manage";
        public const string InvitationsSend = "Invitations.Send";
    }

    /// <summary>
    /// دالة سحرية تستخدم الـ Reflection لجلب جميع الصلاحيات المعرفة في هذا الكلاس.
    /// هذا يمنع خطأ نسيان إضافة صلاحية جديدة إلى قائمة الـ Seed.
    /// </summary>
    public static IReadOnlyList<string> GetAllPermissions()
    {
        var permissions = new List<string>();

        var nestedClasses = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var nestedClass in nestedClasses)
        {
            var constants = nestedClass
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue()!)
                .ToList();

            permissions.AddRange(constants);
        }

        return permissions.AsReadOnly();
    }

    public static class Sessions
    {
        public const string Moderate = "Sessions.Moderate";
        public const string ViewMaterials = "Sessions.ViewMaterials";
        public const string UploadMaterials = "Sessions.UploadMaterials";
        public const string DeleteMaterials = "Sessions.DeleteMaterials";
        public const string ReorderMaterials = "Sessions.ReorderMaterials";
    }
}
