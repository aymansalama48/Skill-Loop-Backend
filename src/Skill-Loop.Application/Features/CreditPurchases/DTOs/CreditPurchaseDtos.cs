namespace Skill_Loop.Application.Features.CreditPurchases.DTOs;

/// <summary>معاينة السعر قبل الدفع (بعد تطبيق كود الخصم لو موجود). المبالغ بأصغر وحدة عملة.</summary>
public sealed record PurchaseQuoteDto(
    int Credits,
    string Currency,
    int SubtotalMinor,
    int DiscountMinor,
    int TotalMinor,
    string? PromoCode);

public sealed record CreditPurchaseDto(
    Guid Id,
    int Credits,
    string Currency,
    int SubtotalMinor,
    int DiscountMinor,
    int TotalMinor,
    string Status,
    string? CheckoutUrl,
    int? NewBalance,
    DateTime CreatedAt);
