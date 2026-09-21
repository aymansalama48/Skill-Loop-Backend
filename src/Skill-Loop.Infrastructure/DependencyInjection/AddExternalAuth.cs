using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Abstractions.Identity.Providers;
using Skill_Loop.Infrastructure.Identity.Providers;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    private static IServiceCollection AddExternalAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));


        services.AddScoped<IExternalAuthProvider, GoogleAuthProvider>();


        return services;
    }
}