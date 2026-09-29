using Skill_Loop.Domain.Common.Entities;

namespace Skill_Loop.Domain.Entities.Promotions;

/// <summary>
/// سجل "المستخدم ده استخدم الكود ده". الـ Unique Index على (PromoCodeId, UserId)
/// بيضمن إن كل مستخدم يستخدم الكود مرة واحدة بس.
/// </summary>
public sealed class PromoRedemption : BaseEntity
{
    public Guid PromoCodeId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PurchaseId { get; private set; }
    public int DiscountMinor { get; private set; }
    public DateTime RedeemedAt { get; private set; }

    private PromoRedemption() { }

    public static PromoRedemption Create(Guid promoCodeId, Guid userId, Guid purchaseId, int discountMinor) => new()
    {
        PromoCodeId = promoCodeId,
        UserId = userId,
        PurchaseId = purchaseId,
        DiscountMinor = discountMinor,
        RedeemedAt = DateTime.UtcNow
    };
}
