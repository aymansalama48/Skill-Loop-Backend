using Skill_Loop.Application.Common.Abstractions.External.Payments;
using Skill_Loop.Domain.Entities.Promotions;

namespace Skill_Loop.Application.Features.CreditPurchases.Common;

internal sealed record PurchaseQuote(int SubtotalMinor, int DiscountMinor, int TotalMinor);

internal static class PurchaseQuoteCalculator
{
    public static PurchaseQuote Calculate(int credits, ICreditPricing pricing, PromoCode? promo)
    {
        var subtotal = checked(credits * pricing.PricePerCreditMinor);
        var discount = promo?.CalculateDiscount(subtotal) ?? 0;
        return new PurchaseQuote(subtotal, discount, subtotal - discount);
    }
}
