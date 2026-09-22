using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Infrastructure.External.Cache;
using StackExchange.Redis;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024;
        });

        var redisConnectionString = configuration.GetSection("Redis:ConnectionString").Value;

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            try
            {
                var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
                redisOptions.AbortOnConnectFail = false;
                redisOptions.ConnectTimeout = 3000;

                var multiplexer = ConnectionMultiplexer.Connect(redisOptions);
                services.AddSingleton<IConnectionMultiplexer>(multiplexer);
                services.AddSingleton<ICacheService, RedisCacheService>();
                return services;
            }
            catch
            {
                // Graceful fallback to MemoryCache if Redis is unreachable during startup
            }
        }

        services.AddSingleton<ICacheService, MemoryCacheService>();
        return services;
    }
}