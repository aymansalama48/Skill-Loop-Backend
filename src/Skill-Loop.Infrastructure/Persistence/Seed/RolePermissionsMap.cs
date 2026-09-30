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
                // An instructor owns their sessions and must be able to move one from Draft to
                // Published, otherwise it can never be booked. Sessions.Moderate is the
                // permission guarding PATCH /api/v1/Sessions/{id}/status.
                Permissions.Sessions.Moderate,
                // Same reasoning for viewing the roster of bookings against their own
                // session. The endpoint is scoped by sessionId, so this cannot be used to
                // read bookings across instructors.
                Permissions.Bookings.ViewAll,
                Permissions.Dashboards.ViewInstructor
            },

            [Roles.User] = new string[]
            {
                // Users rely on [AuthenticatedOnly] and [AllowAnonymous] for their endpoints.
                // No specific permissions are required to manage their own data.
                //
                // Bookings.View is the exception: GET /api/v1/Bookings/{id} is gated on it so
                // that it cannot be used as a blanket "read any booking" permission. The
                // handler still enforces ownership - the learner who booked, the session's
                // instructor, or a Bookings.ViewAll/ManageAll holder. Without this grant the
                // gate rejected the learner before the ownership check could pass them.
                Permissions.Bookings.View
            }
        };
}