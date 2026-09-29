using Microsoft.Extensions.Options;
using Skill_Loop.Application.Common.Abstractions.External.Payments;
using Skill_Loop.Infrastructure.Options;

namespace Skill_Loop.Infrastructure.External.Payments;

internal sealed class ConfiguredCreditPricing(IOptions<PaymentOptions> options) : ICreditPricing
{
    public int PricePerCreditMinor => options.Value.PricePerCreditMinor;
    public string Currency => options.Value.Currency;
}
