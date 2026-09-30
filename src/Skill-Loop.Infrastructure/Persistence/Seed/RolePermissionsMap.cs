using Skill_Loop.Domain.Constants;
using System.Linq;

namespace Skill_Loop.Infrastructure.Persistence.Seed;

public static class RolePermissionsMap
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Map =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [Roles.SuperAdmin] = Permissions.GetAllPermissions(),

            [Roles.Admin] = Permissions.GetAllPermissions()
                .Where(p => !p.StartsWith("Access.")) // Admin has everything except Access management (SuperAdmin only)
                .ToList(),

            [Roles.FinanceManager] = new[]
            {
                Permissions.Finance.View,
                Permissions.Finance.PackagesManage,
                Permissions.Finance.PromoCodesManage,
                Permissions.Finance.WalletAdjust,
                Permissions.Finance.PaymentsView,
                Permissions.Finance.ManageAll,
                Permissions.Dashboards.ViewAdmin
            },

            [Roles.Support] = new[]
            {
                Permissions.Users.View,
                Permissions.Bookings.ViewAll,
                Permissions.Finance.View,
                Permissions.Support.View,
                Permissions.Support.Manage
            },

            [Roles.Instructor] = new[]
            {
                Permissions.Courses.View,
                Permissions.Courses.Create,
                Permissions.Courses.Update,
                Permissions.Courses.Delete,
                Permissions.Courses.Publish,
                Permissions.Courses.Archive,
                Permissions.Sessions.View,
                Permissions.Sessions.Create,
                Permissions.Sessions.Update,
                Permissions.Sessions.Delete,
                Permissions.Sessions.Cancel,
                Permissions.Sessions.ViewMaterials,
                Permissions.Sessions.UploadMaterials,
                Permissions.Sessions.DeleteMaterials,
                Permissions.Sessions.ReorderMaterials,
                Permissions.Dashboards.ViewInstructor
            },

            [Roles.User] = new string[]
            {
                // Users rely on [AuthenticatedOnly] and [AllowAnonymous] for their endpoints.
                // No specific permissions are required to manage their own data.
            }
        };
}