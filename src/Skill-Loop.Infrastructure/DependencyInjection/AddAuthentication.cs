using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Skill_Loop.Infrastructure.Options;
using System.Text;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    /// <summary>
    /// تسجيل إعدادات وخدمات التوثيق والـ JWT Bearer
    /// </summary>
    private static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtOptions.SectionName);

        // 1. ربط ملف الإعدادات
        services.Configure<JwtOptions>(section);

        // 2. القراءة المباشرة (بدون إخفاء الأخطاء)
        var jwtKey = section["Key"];
        var jwtIssuer = section["Issuer"];
        var jwtAudience = section["Audience"];

        // 🚨 حائط الصد: لو المفتاح مقريش السيرفر هيقف ويفضح المشكلة
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("💥 خطأ قاتل: المفتاح السري (Jwt:Key) غير موجود أو لم يتم قراءته من appsettings.json!");
        }

        // 3. إعداد الـ Authentication Scheme
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,

                    // هنا بنمرر المفتاح السليم غصب عنه
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
