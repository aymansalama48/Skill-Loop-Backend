using System.Reflection;

namespace Skill_Loop.Domain.Constants;

public class Permissions
{
    public static class Courses
    {
        public const string View = "Courses.View";
        public const string Create = "Courses.Create";
        public const string Update = "Courses.Update";
        public const string Delete = "Courses.Delete";
        public const string Publish = "Courses.Publish";
        public const string Archive = "Courses.Archive";
        public const string ManageAll = "Courses.ManageAll";
    }

    public static class Sessions
    {
        public const string View = "Sessions.View";
        public const string Create = "Sessions.Create";
        public const string Update = "Sessions.Update";
        public const string Cancel = "Sessions.Cancel";
        public const string Delete = "Sessions.Delete";
        public const string Moderate = "Sessions.Moderate";
        public const string ViewMaterials = "Sessions.ViewMaterials";
        public const string UploadMaterials = "Sessions.UploadMaterials";
        public const string DeleteMaterials = "Sessions.DeleteMaterials";
        public const string ReorderMaterials = "Sessions.ReorderMaterials";
        public const string ManageAll = "Sessions.ManageAll";
    }

    public static class Bookings
    {
        public const string View = "Bookings.View";
        public const string ViewAll = "Bookings.ViewAll";
        public const string ManageAll = "Bookings.ManageAll";
    }

    public static class Users
    {
        public const string View = "Users.View";
        public const string Activate = "Users.Activate";
        public const string Deactivate = "Users.Deactivate";
        public const string AssignRole = "Users.AssignRole";
        public const string ManageAll = "Users.ManageAll";
    }

    public static class Access
    {
        public const string RolesManage = "Roles.Manage";
        public const string PermissionsManage = "Permissions.Manage";
        public const string InvitationsSend = "Invitations.Send";
    }

    public static class Finance
    {
        public const string View = "Finance.View";
        public const string PackagesManage = "Packages.Manage";
        public const string PromoCodesManage = "PromoCodes.Manage";
        public const string WalletAdjust = "Wallet.Adjust";
        public const string PaymentsView = "Payments.View";
        public const string ManageAll = "Finance.ManageAll";
    }

    public static class Dashboards
    {
        public const string ViewAdmin = "Dashboards.ViewAdmin";
        public const string ViewInstructor = "Dashboards.ViewInstructor";
    }

    public static class Catalog
    {
        public const string CategoriesManage = "Categories.Manage";
        public const string SkillsManage = "Skills.Manage";
        public const string TagsManage = "Tags.Manage";
    }

    public static class SiteSettings
    {
        public const string Manage = "SiteSettings.Manage";
    }

    public static class Support
    {
        public const string View = "Support.View";
        public const string Manage = "Support.Manage";
    }

    public static class Instructors
    {
        public const string View = "Instructors.View";
        public const string ManageAll = "Instructors.ManageAll";
    }

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
}
