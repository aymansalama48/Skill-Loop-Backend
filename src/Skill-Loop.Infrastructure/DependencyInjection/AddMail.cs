using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Abstractions.External.Email;
using Skill_Loop.Infrastructure.External.Email;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    /// <summary>
    /// تسجيل خدمات البريد الإلكتروني (Email Services).
    /// </summary>
    public static IServiceCollection AddMail(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // إعدادات الـ Mail
        services.Configure<MailOptions>(configuration.GetSection(MailOptions.SectionName));

        // تسجيل محرك القوالب وخدمة الإرسال
        services.AddScoped<IEmailTemplateEngine, EmailTemplateEngine>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
