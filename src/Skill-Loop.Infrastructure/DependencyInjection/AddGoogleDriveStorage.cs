using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.Storage;
using Skill_Loop.Infrastructure.External.Storage;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection AddGoogleDriveStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GoogleDriveOptions>(
            configuration.GetSection(GoogleDriveOptions.SectionName));

        services.AddScoped<ICourseContentStorage, GoogleDriveContentStorage>();

        return services;
    }
}