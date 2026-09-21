using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Abstractions.Identity.Security;
using Skill_Loop.Infrastructure.Identity.Security;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection AddOtpService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. ربط الإعدادات من appsettings.json بـ OtpOptions
        services.Configure<OtpOptions>(configuration.GetSection(OtpOptions.SectionName));

        // 2. تسجيل الخدمة IOtpService مع OtpService
        services.AddScoped<IOtpService, OtpService>();

        return services;
    }
}