using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Infrastructure.External.Cache;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    private static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache(options =>
        {
            // حد أقصى مثلاً 1024 وحدة (عنصر) داخل الكاش
            options.SizeLimit = 1024;
        });
        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }
}