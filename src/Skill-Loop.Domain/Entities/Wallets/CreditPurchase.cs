using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Wallets;

/// <summary>
/// عملية شراء كريديت بفلوس حقيقية. كل المبالغ بأصغر وحدة عملة (قرش/سنت) عشان نتجنب
/// مشاكل الكسور العشرية.
/// </summary>
public sealed class CreditPurchase : AuditableEntity
{
    public const int MinCredits = 1;
    public const int MaxCredits = 10_000;

    public Guid UserId { get; private set; }
    public int Credits { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    public int SubtotalMinor { get; private set; }
    public int DiscountMinor { get; private set; }
    public int TotalMinor { get; private set; }

    public Guid? PromoCodeId { get; private set; }
    public CreditPurchaseStatus Status { get; private set; }

    /// <summary>رقم العملية عند بوابة الدفع (Stripe/Paymob...).</summary>
    public string? GatewayReference { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private CreditPurchase() { }

    public static Result<CreditPurchase> Create(
        Guid userId,
        int credits,
        string currency,
        int subtotalMinor,
        int discountMinor,
        int totalMinor,
        Guid? promoCodeId)
    {
        if (userId == Guid.Empty)
            return Result<CreditPurchase>.Failure(new Error("Purchase.InvalidUser", "المستخدم مطلوب.", ErrorType.Validation));

        if (credits is < MinCredits or > MaxCredits)
            return Result<CreditPurchase>.Failure(new Error(
                "Purchase.InvalidCredits",
                $"عدد الكريديت لازم يكون بين {MinCredits} و {MaxCredits}.",
                ErrorType.Validation));

        if (subtotalMinor < 0 || discountMinor < 0 || totalMinor < 0 || subtotalMinor - discountMinor != totalMinor)
            return Result<CreditPurchase>.Failure(new Error(
                "Purchase.InvalidAmounts", "المبالغ غير صحيحة.", ErrorType.Validation));

        return Result<CreditPurchase>.Success(new CreditPurchase
        {
            UserId = userId,
            Credits = credits,
            Currency = currency,
            SubtotalMinor = subtotalMinor,
            DiscountMinor = discountMinor,
            TotalMinor = totalMinor,
            PromoCodeId = promoCodeId,
            Status = CreditPurchaseStatus.Pending
        });
    }

    public void SetGatewayReference(string reference) => GatewayReference = reference;

    public void MarkCompleted(string gatewayReference)
    {
        GatewayReference = gatewayReference;
        Status = CreditPurchaseStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed() => Status = CreditPurchaseStatus.Failed;
}
