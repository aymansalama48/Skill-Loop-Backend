namespace Skill_Loop.Api.Extensions;

public static class CorsExtensions
{
    /// <summary>
    /// إضافة وتجهيز سياسة CORS بناءً على الإعدادات المحددة في appsettings.json
    ///
    /// Security: this policy uses AllowCredentials together with an explicit origin
    /// allowlist. A wildcard is therefore never valid here — with credentials, "*" would
    /// let any website read authenticated API responses. An empty allowlist is rejected
    /// rather than defaulted, because silently defaulting to a localhost origin hides a
    /// misconfiguration until it becomes a production outage.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                             ?? [];

        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException(
                "CorsSettings:AllowedOrigins is empty. Set an explicit list of frontend " +
                "origins. A wildcard is not supported because this policy allows credentials.");
        }

        if (allowedOrigins.Contains("*", StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "CorsSettings:AllowedOrigins contains '*', which cannot be combined with " +
                "AllowCredentials. Listing the wildcard here would let any website make " +
                "authenticated requests to this API and read the responses.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // للسماح بتمرير الـ Cookies أو الـ Headers الخاصة بالتوثيق
            });
        });

        return services;
    }
}