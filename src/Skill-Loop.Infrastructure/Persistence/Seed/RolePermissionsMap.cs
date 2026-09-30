using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Infrastructure.Persistence.Seed;

public static class RolePermissionsMap
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Map =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [Roles.SuperAdmin] = Permissions.GetAllPermissions(),

            [Roles.Admin] = new[]
            {
                Permissions.Courses.ManageAll,
                Permissions.Sessions.ManageAll,
                Permissions.Bookings.ManageAll,
                Permissions.Users.ManageAll,
                Permissions.Finance.ManageAll,
                Permissions.Dashboards.ViewAdmin,
                Permissions.Catalog.CategoriesManage,
                Permissions.Catalog.SkillsManage,
                Permissions.Catalog.TagsManage,
                Permissions.SiteSettings.Manage,
                Permissions.Support.Manage,
                Permissions.Instructors.ManageAll
            },

            [Roles.FinanceManager] = new[]
            {
                Permissions.Finance.ManageAll,
                Permissions.Finance.PackagesManage,
                Permissions.Finance.PromoCodesManage,
                Permissions.Finance.WalletAdjust,
                Permissions.Finance.PaymentsView,
                Permissions.Dashboards.ViewAdmin
            },

            [Roles.Support] = new[]
            {
                Permissions.Users.View,
                Permissions.Bookings.ViewAll,
                Permissions.Finance.View,
                Permissions.Support.Manage
            },

            [Roles.Instructor] = new[]
            {
                Permissions.Courses.Create,
                Permissions.Courses.Update,
                Permissions.Courses.Delete,
                Permissions.Courses.Publish,
                Permissions.Courses.Archive,
                Permissions.Sessions.Create,
                Permissions.Sessions.Update,
                Permissions.Sessions.Delete,
                Permissions.Sessions.Cancel,
                Permissions.Sessions.UploadMaterials,
                Permissions.Sessions.DeleteMaterials,
                Permissions.Sessions.ReorderMaterials,
                Permissions.Dashboards.ViewInstructor
            },

            [Roles.User] = new string[]
            {
                // Users mainly rely on [AuthenticatedOnly] for their own data.
            }
        };
}