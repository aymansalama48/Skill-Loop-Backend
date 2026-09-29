using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skill_Loop.Application.Common.Abstractions.External.Payments;
using Skill_Loop.Infrastructure.External.Payments;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.DependencyInjection;

public static partial class DependencyInjection
{
    public static IServiceCollection AddPayments(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaymentOptions>(configuration.GetSection(PaymentOptions.SectionName));

        var options = configuration.GetSection(PaymentOptions.SectionName).Get<PaymentOptions>() ?? new PaymentOptions();

        if (options.PricePerCreditMinor is < 1 or > 100_000)
            throw new InvalidOperationException("Payments:PricePerCreditMinor لازم يكون بين 1 و 100000.");

        services.AddSingleton<ICreditPricing, ConfiguredCreditPricing>();

        switch (options.Gateway.Trim().ToLowerInvariant())
        {
            case "fake":
                // 🔒 حماية: مينفعش بوابة بتدّي كريديت ببلاش تشتغل على السيرفر الحقيقي
                var environment = configuration["ASPNETCORE_ENVIRONMENT"];
                if (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "Payments:Gateway = Fake غير مسموح في Production. ركّب بوابة دفع حقيقية.");

                services.AddScoped<IPaymentGateway, FakePaymentGateway>();
                break;

            default:
                throw new InvalidOperationException(
                    $"بوابة الدفع '{options.Gateway}' غير مدعومة لسه. المتاح حالياً: Fake.");
        }

        return services;
    }
}
