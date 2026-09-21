using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    /// <summary>
    /// تسجيل إعدادات التطبيق العامة (App Settings Options).
    /// </summary>
    public static IServiceCollection AddBaseUrl(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // تسجيل الـ Options Pattern الخاص بإعدادات التطبيق العامة (مثل الـ BaseUrl)
        services.Configure<BaseUrlOptions>(configuration.GetSection(BaseUrlOptions.SectionName));

        return services;
    }
}
