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
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(
                configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
        });

        services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        services.AddTransient<RefreshDriveQuotaJob>();
        services.AddTransient<ProcessPendingEmailsJob>();

        return services;
    }
}