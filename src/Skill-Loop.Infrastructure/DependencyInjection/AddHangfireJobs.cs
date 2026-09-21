using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Infrastructure.External.Jobs;

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
        // 1. إعداد الـ Storage والـ Serializer الخاص بـ Hangfire
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

        // 2. تسجيل الـ Hangfire Processing Server في الخلفية
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2; // عدد الـ Workers بناءً على المعالج
        });

        // 3. تسجيل الـ Job Scheduler الخاص بنا والذي يستخدم Hangfire
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        return services;
    }
}