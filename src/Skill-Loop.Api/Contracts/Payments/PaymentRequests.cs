using Skill_Loop.Api.Contracts.Common;

namespace Skill_Loop.Api.Contracts.Payments;

/// <summary>DiscountType: "Percentage" (DiscountValue = 1..100) أو "FixedAmount" (DiscountValue بأصغر وحدة عملة).</summary>
public sealed record CreatePromoCodeRequest(
    string Code,
    string DiscountType,
    int DiscountValue,
    int? MaxRedemptions,
    DateTime? ExpiresAt);

public sealed record GetPromoCodesRequest : PaginationRequest;

public sealed record PurchaseQuoteRequest(int Credits, string? PromoCode);

public sealed record PurchaseCreditsRequest(int Credits, string? PromoCode);
