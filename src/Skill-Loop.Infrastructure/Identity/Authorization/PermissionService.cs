using Microsoft.AspNetCore.Identity;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using Microsoft.EntityFrameworkCore;

namespace Skill_Loop.Infrastructure.Identity.Authorization;

/// <summary>
/// تنفيذ خدمة الصلاحيات (Role-based) — بيجيب صلاحيات اليوزر من الأدوار المرتبطة بيه
/// </summary>
public class PermissionService(
    UserManager<ApplicationUser> userManager,
    AppDbContext context) : IPermissionService
{
    /// <summary>
    /// التحقق من إن اليوزر معاه صلاحية معينة
    /// </summary>
    public async Task<bool> HasPermissionAsync(
        Guid userId,
        string permissionName,
        CancellationToken cancellationToken)
    {
        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
        return permissions.Contains(permissionName);
    }

    /// <summary>
    /// جلب كل صلاحيات اليوزر (مجمعة من كل أدواره)
    /// </summary>
    public async Task<IList<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return new List<string>();

        var roleNames = await userManager.GetRolesAsync(user);
        if (roleNames.Count == 0)
            return new List<string>();

        var permissions = await context.Roles   // 👈 دلوقتي بترجع ApplicationRole صح بعد تصحيح AppDbContext
            .Where(r => roleNames.Contains(r.Name!))
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions;
    }
}