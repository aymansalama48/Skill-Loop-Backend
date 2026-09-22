using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Persistence.Seed;

public static class ContextSeed
{
    public static async Task SeedRolesAndPermissionsAsync(
        RoleManager<ApplicationRole> roleManager,
        DbContext dbContext)
    {
        // 1. إنشاء الأدوار الأساسية في النظام
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole(roleName, $"Default system role for {roleName}")
                {
                    IsSystemRole = true
                };
                await roleManager.CreateAsync(role);
            }
        }

        // 2. تعبئة جدول الصلاحيات (TbPermission)
        var existingPermissionNames = await dbContext.Set<TbPermission>()
            .Select(p => p.Name)
            .ToListAsync();

        var newPermissions = new List<TbPermission>();

        var allPermissionsList = Permissions.GetAllPermissions();

        foreach (var permissionCode in allPermissionsList)
        {
            if (!existingPermissionNames.Contains(permissionCode))
            {
                var parts = permissionCode.Split('.');
                var module = parts.Length > 0 ? parts[0] : "General";
                var action = parts.Length > 1 ? parts[1] : permissionCode;

                newPermissions.Add(new TbPermission
                {
                    Name = permissionCode,
                    DisplayName = $"{action} in {module}",
                    Module = module,
                    Description = $"Grants access to {permissionCode}"
                });
            }
        }

        if (newPermissions.Any())
        {
            await dbContext.Set<TbPermission>().AddRangeAsync(newPermissions);
            await dbContext.SaveChangesAsync();
        }

        // 3. ربط الصلاحيات بالأدوار حسب الـ RolePermissionsMap
        await LinkPermissionsToRolesAsync(roleManager, dbContext);
    }

    /// <summary>
    /// يربط كل دور بالصلاحيات المحددة له في RolePermissionsMap.
    /// آمن للتشغيل المتكرر (Idempotent) — بيتخطى الصلاحيات المربوطة بالفعل.
    /// </summary>
    private static async Task LinkPermissionsToRolesAsync(
        RoleManager<ApplicationRole> roleManager,
        DbContext dbContext)
    {
        // نجيب كل الصلاحيات من الـ DB ونعمل lookup بالـ Name
        var allPermissions = await dbContext.Set<TbPermission>().ToListAsync();
        var permissionsByName = allPermissions.ToDictionary(p => p.Name, p => p.Id);

        foreach (var (roleName, permissionCodes) in RolePermissionsMap.Map)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            // الصلاحيات المربوطة بالدور بالفعل
            var existingRolePermissionIds = await dbContext.Set<TbRolePermission>()
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            // الصلاحيات المطلوب ربطها
            var targetPermissionIds = permissionCodes
                .Where(permissionsByName.ContainsKey)
                .Select(code => permissionsByName[code])
                .ToList();

            // اللي محتاج يتضاف بس
            var toAdd = targetPermissionIds
                .Where(pid => !existingRolePermissionIds.Contains(pid))
                .Select(pid => new TbRolePermission
                {
                    RoleId = role.Id,
                    PermissionId = pid,
                    GrantedAt = DateTime.UtcNow
                })
                .ToList();

            if (toAdd.Any())
            {
                await dbContext.Set<TbRolePermission>().AddRangeAsync(toAdd);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}