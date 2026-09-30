using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Abstractions.Persistence.Transaction;
using Skill_Loop.Infrastructure.BackgroundJobs;
using Skill_Loop.Infrastructure.Persistence.Data;
using Skill_Loop.Infrastructure.Persistence.Interceptors;
using Skill_Loop.Infrastructure.Persistence.Transaction;

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

            options.UseSqlServer(
                       configuration.GetConnectionString("DefaultConnection"),
                       b =>
                       {
                           b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                           // 👇 السطر ده هو اللي هيحل مشكلة الـ Docker Migrations
                           b.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                       })
                   .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
                   .AddInterceptors(
                       softDeleteInterceptor,
                       auditableInterceptor,
                       insertOutboxInterceptor);
        });

        // تسجيل الـ Adapter ليربط الواجهة بالكلاس الجديد
        services.AddScoped<IApplicationDbContext, ApplicationDbContextAdapter>();

        // EfTransactionManager was never registered, so ITransactionManager could not be
        // resolved. That stayed invisible while TransactionBehavior<,> was effectively dead
        // (it constrains TResponse : Result, while commands are declared as
        // ICommand<Guid> / ICommand<bool>, so MediatR never selected it). Once
        // NonGenericCommandTransactionBehavior - which constrains only TRequest : ICommand -
        // was registered, it WAS selected, and every non-generic command failed at
        // resolution time with a 500. Registering the implementation is the real fix.
        services.AddScoped<ITransactionManager, EfTransactionManager>();

        // تسجيل الكلاس كـ Scoped:
        services.AddScoped<ProcessOutboxMessagesJob>();

        return services;
    }
}