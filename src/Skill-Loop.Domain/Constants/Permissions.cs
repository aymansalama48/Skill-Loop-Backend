using System.Reflection;

namespace Skill_Loop.Domain.Constants;

public class Permissions
{
    /// <summary>
    /// دالة سحرية تستخدم الـ Reflection لجلب جميع الصلاحيات المعرفة في هذا الكلاس.
    /// هذا يمنع خطأ نسيان إضافة صلاحية جديدة إلى قائمة الـ Seed.
    /// </summary>
    public static IReadOnlyList<string> GetAllPermissions()
    {
        var permissions = new List<string>();

        // نجيب كل الكلاسات الداخلية (Nested Classes)
        var nestedClasses = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var nestedClass in nestedClasses)
        {
            // نجيب كل الثوابت (Constants) جوه الكلاس ده
            var constants = nestedClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
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
