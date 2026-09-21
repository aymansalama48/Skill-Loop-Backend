using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Infrastructure.BackgroundJobs;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.Infrastructure.Persistence.Interceptors;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. تسجيل الـ Interceptors الخاصة بـ Entity Framework
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<InsertOutboxMessagesInterceptor>();

        // 2. تسجيل الـ DbContext وربطه بـ SQL Server
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var insertOutboxInterceptor = sp.GetRequiredService<InsertOutboxMessagesInterceptor>();

            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(
                       softDeleteInterceptor,
                       auditableInterceptor,
                       insertOutboxInterceptor);
        });



        // 👇 تسجيل الـ Adapter ليربط الواجهة بالكلاس الجديد
        services.AddScoped<IApplicationDbContext, ApplicationDbContextAdapter>();


        // ✅ واكتب مكانه تسجيل الكلاس كـ Scoped:
        services.AddScoped<ProcessOutboxMessagesJob>();


        return services;
    }
}