using Skill_Loop.Domain.Entities.Promotions;

namespace Skill_Loop.Application.Features.Promotions.DTOs;

public sealed record PromoCodeDto(
    Guid Id,
    string Code,
    string DiscountType,
    int DiscountValue,
    int? MaxRedemptions,
    int RedemptionsCount,
    DateTime? ExpiresAt,
    bool IsActive,
    DateTime CreatedAt);

public static class PromoCodeMappings
{
    public static PromoCodeDto ToDto(this PromoCode p) => new(
        p.Id, p.Code, p.DiscountType.ToString(), p.DiscountValue,
        p.MaxRedemptions, p.RedemptionsCount, p.ExpiresAt, p.IsActive, p.CreatedAt);
}
