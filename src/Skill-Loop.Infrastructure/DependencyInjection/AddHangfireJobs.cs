using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Infrastructure.BackgroundJobs;
using Skill_Loop.Infrastructure.External.Jobs;
using Skill_Loop.Infrastructure.External.Storage;
using Skill_Loop.Infrastructure.Notifications;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    /// <summary>
    /// تسجيل وإعداد خدمات Hangfire لإدارة المهام في الخلفية
    /// </summary>
    private static IServiceCollection AddHangfireJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Hangfire needs its own database. Sharing the application database means the
        // background job worker competes with live requests for the same connection pool
        // and locks, and a runaway job can block application queries. Falling back to the
        // application connection would silently reintroduce that coupling, so a missing
        // dedicated connection is a startup error instead.
        var hangfireConnection = configuration.GetConnectionString("HangfireConnection");

        if (string.IsNullOrWhiteSpace(hangfireConnection))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:HangfireConnection is not configured. Hangfire requires a " +
                "dedicated database separate from the application database; supply it via " +
                "user-secrets or the ConnectionStrings__HangfireConnection environment variable.");
        }

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(
                hangfireConnection,
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    // QueuePollInterval = Zero كان بيعمل tight loop على الـ SQL Server
                    // (استعلامات لا نهائية على جدول الـ queue). القيمة الموصى بها من Hangfire هي 15 ثانية.
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

        services.AddHangfireServer(options =>
        {
            // Capping the worker count prevents a burst of scheduled jobs from saturating
            // the database on a small instance.
            options.WorkerCount = Math.Clamp(Environment.ProcessorCount * 2, 2, 20);
        });

        services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        services.AddTransient<RefreshDriveQuotaJob>();
        services.AddTransient<ProcessPendingEmailsJob>();

        return services;
    }
}