using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.Infrastructure.Persistence.IdentityModels;
using Skill_Loop.Infrastructure.Persistence.Seed;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    /// <summary>
    /// تشغيل عملية الـ Seeding للـ Database والـ Roles والـ Permissions عند بداية التطبيق
    /// </summary>
    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            var dbContext = services.GetRequiredService<AppDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            // 1. تطبيق أي Migrations معلقة تلقائياً
            await dbContext.Database.MigrateAsync();

            // 2. تشغيل الـ Seed الخاص بالـ Roles والـ Permissions
            await ContextSeed.SeedRolesAndPermissionsAsync(roleManager, dbContext);

            logger.LogInformation("Database Seeding executed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}