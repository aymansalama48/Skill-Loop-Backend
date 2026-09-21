using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Persistence.Seed;

public static class ContextSeed
{
    public static async Task SeedRolesAndPermissionsAsync(
        RoleManager<ApplicationRole> roleManager,
        DbContext dbContext) // pass ApplicationDbContext here
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

        // 👇 التعديل هنا: استخدام الدالة السحرية اللي عملناها
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

        // 3. ربط جميع الصلاحيات بدور الـ Admin تلقائياً
        var adminRole = await roleManager.FindByNameAsync(Roles.Admin);
        if (adminRole != null)
        {
            var allPermissions = await dbContext.Set<TbPermission>().ToListAsync();
            var existingRolePermissions = await dbContext.Set<TbRolePermission>()
                .Where(rp => rp.RoleId == adminRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var newRolePermissions = allPermissions
                .Where(p => !existingRolePermissions.Contains(p.Id))
                .Select(p => new TbRolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = p.Id,
                    GrantedAt = DateTime.UtcNow
                })
                .ToList();

            if (newRolePermissions.Any())
            {
                await dbContext.Set<TbRolePermission>().AddRangeAsync(newRolePermissions);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
