using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Domain.Entities.SiteSettings;
using Skill_Loop.Infrastructure.Persistence.IdentityModels; // تأكد أن ApplicationUser موجود هنا

namespace Skill_Loop.Infrastructure.Persistence.Seed;

public static class ContextSeed
{
    // =================================================================================
    // 1. إضافة الإعدادات الافتراضية للموقع (Site Settings)
    // =================================================================================
    public static async Task SeedSiteSettingsAsync(DbContext dbContext)
    {
        // التحقق مما إذا كان الجدول فارغاً
        if (!await dbContext.Set<SiteSettings>().AnyAsync())
        {
            var defaultSettings = new SiteSettings
            {
                AppName = "Skill Loop",
                SupportEmail = "support@skillloop.com",
                ContactPhoneNumber = "01000000000",
                WebsiteUrl = "https://skillloop.com",
                Address = "Cairo, Egypt",
                FacebookUrl = "https://facebook.com/skillloop",
                InstagramUrl = "https://instagram.com/skillloop",
                WhatsAppNumber = "01000000000",
                LogoName = "default-logo.png"
            };

            await dbContext.Set<SiteSettings>().AddAsync(defaultSettings);
            await dbContext.SaveChangesAsync();
        }
    }

    // =================================================================================
    // 2. إضافة حساب السوبر آدمن الافتراضي
    // =================================================================================
    public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager)
    {
        // بيانات الحساب الافتراضي (يمكنك تغييرها)
        var superAdminEmail = "admin@skillloop.com";
        var defaultPassword = "Password@123";

        // التحقق مما إذا كان الحساب موجوداً بالفعل
        var existingUser = await userManager.FindByEmailAsync(superAdminEmail);

        if (existingUser == null)
        {
            var superAdminUser = new ApplicationUser
            {
                FirstName = "Super",
                LastName = "Admin",
                UserName = superAdminEmail,
                Email = superAdminEmail,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            // إنشاء الحساب
            var result = await userManager.CreateAsync(superAdminUser, defaultPassword);

            if (result.Succeeded)
            {
                // منحه صلاحية SuperAdmin
                await userManager.AddToRoleAsync(superAdminUser, Roles.SuperAdmin);
            }
        }
    }

    // =================================================================================
    // 3. الدوال القديمة: بناء الأدوار والصلاحيات (بدون تغيير)
    // =================================================================================
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

    private static async Task LinkPermissionsToRolesAsync(
        RoleManager<ApplicationRole> roleManager,
        DbContext dbContext)
    {
        var allPermissions = await dbContext.Set<TbPermission>().ToListAsync();
        var permissionsByName = allPermissions.ToDictionary(p => p.Name, p => p.Id);

        foreach (var (roleName, permissionCodes) in RolePermissionsMap.Map)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            var existingRolePermissionIds = await dbContext.Set<TbRolePermission>()
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var targetPermissionIds = permissionCodes
                .Where(permissionsByName.ContainsKey)
                .Select(code => permissionsByName[code])
                .ToList();

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