using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Domain.Entities.SiteSettings;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;

namespace Skill_Loop.Infrastructure.Persistence.Seed;

public static class ContextSeed
{
    // =================================================================================
    // 1. إضافة الإعدادات الافتراضية للموقع (Site Settings)
    // =================================================================================
    public static async Task SeedSiteSettingsAsync(DbContext dbContext)
    {
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

    /// <summary>
    /// Seeds the bootstrap SuperAdmin account.
    ///
    /// Security: this used to create the account with a hardcoded password
    /// ("Password@123") checked into the repository. Anyone with read access to the repo
    /// knew the credentials of the highest-privilege account, and the account was created
    /// automatically on every environment — so a forgotten production deployment came up
    /// with a publicly known admin login.
    ///
    /// The password must now be supplied through configuration (user-secrets in
    /// development, an environment variable elsewhere). Seeding is skipped with a warning
    /// when no password is configured, so a deployment without one simply gets no admin
    /// account rather than a guessable one. An existing account is never touched, so
    /// rotating the configured password does not overwrite a real one.
    /// </summary>
    public static async Task SeedSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var superAdminEmail =
            configuration["Seed:SuperAdmin:Email"]?.Trim();

        var configuredPassword = configuration["Seed:SuperAdmin:Password"];

        if (string.IsNullOrWhiteSpace(superAdminEmail))
        {
            logger.LogWarning(
                "SuperAdmin seeding skipped: Seed:SuperAdmin:Email is not configured.");
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(superAdminEmail);

        if (existingUser is not null)
        {
            logger.LogInformation(
                "SuperAdmin {Email} already exists; leaving its credentials untouched.",
                superAdminEmail);
            return;
        }

        if (string.IsNullOrWhiteSpace(configuredPassword))
        {
            // Refuse to invent a password. A random one nobody captured would lock the
            // operator out; a well-known one would be worse than no account at all.
            logger.LogError(
                "SuperAdmin seeding skipped: Seed:SuperAdmin:Password is not configured. " +
                "Set it (user-secrets in development, Seed__SuperAdmin__Password in " +
                "production) and restart to create {Email}.",
                superAdminEmail);
            return;
        }

        var superAdminUser = new ApplicationUser
        {
            FirstName = configuration["Seed:SuperAdmin:FirstName"] ?? "Super",
            LastName = configuration["Seed:SuperAdmin:LastName"] ?? "Admin",
            UserName = superAdminEmail,
            Email = superAdminEmail,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };

        var result = await userManager.CreateAsync(superAdminUser, configuredPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            // The password is never logged, only the failure reason.
            logger.LogError(
                "Failed to seed SuperAdmin {Email}: {Errors}", superAdminEmail, errors);
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(superAdminUser, Roles.SuperAdmin);

        if (!roleResult.Succeeded)
        {
            logger.LogError(
                "SuperAdmin {Email} was created but the role assignment failed: {Errors}. " +
                "Assign the SuperAdmin role manually before using the account.",
                superAdminEmail,
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            return;
        }

        logger.LogInformation("SuperAdmin {Email} seeded successfully.", superAdminEmail);
    }

    // =================================================================================
    // 3. بناء الأدوار والصلاحيات
    // =================================================================================
    public static async Task SeedRolesAndPermissionsAsync(
        RoleManager<ApplicationRole> roleManager,
        DbContext dbContext,
        Skill_Loop.Application.Common.Abstractions.Core.IDateTime dateTimeProvider)
    {
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

        var existingPermissionNames = await dbContext.Set<TbPermission>()
            .Select(p => p.Name)
            .ToListAsync();

        var newPermissions = new List<TbPermission>();
        var allPermissionsList = Permissions.GetAllPermissions();

        foreach (var permissionCode in allPermissionsList)
        {
            if (!existingPermissionNames.Contains(permissionCode) &&
                !newPermissions.Any(p => p.Name == permissionCode))
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

        await LinkPermissionsToRolesAsync(roleManager, dbContext, dateTimeProvider);
    }

    private static async Task LinkPermissionsToRolesAsync(
        RoleManager<ApplicationRole> roleManager,
        DbContext dbContext,
        Skill_Loop.Application.Common.Abstractions.Core.IDateTime dateTimeProvider)
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

            // 👈 إضافة .Distinct() هنا لمنع تكرار المعرفات لنفس الدور
            var targetPermissionIds = permissionCodes
                .Where(permissionsByName.ContainsKey)
                .Select(code => permissionsByName[code])
                .Distinct()
                .ToList();

            var toAdd = targetPermissionIds
                .Where(pid => !existingRolePermissionIds.Contains(pid))
                .Select(pid => new TbRolePermission
                {
                    RoleId = role.Id,
                    PermissionId = pid,
                    GrantedAt = dateTimeProvider.Now
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