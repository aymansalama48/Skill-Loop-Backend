namespace Skill_Loop.Api.Extensions;

public static class CorsExtensions
{
    /// <summary>
    /// إضافة وتجهيز سياسة CORS بناءً على الإعدادات المحددة في appsettings.json
    /// </summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // قراءة النطاقات المسموحة من ملف الإعدادات
        var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                             ?? new[] { "http://localhost:3000" };

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