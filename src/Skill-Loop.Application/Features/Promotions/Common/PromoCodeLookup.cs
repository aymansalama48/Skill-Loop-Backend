using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Promotions;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Promotions;

namespace Skill_Loop.Application.Features.Promotions.Common;

/// <summary>
/// منطق "الكود ده ينفع المستخدم ده يستخدمه؟" في مكان واحد، عشان معاينة السعر
/// والشراء الفعلي يطبّقوا نفس القواعد بالظبط.
/// </summary>
internal static class PromoCodeLookup
{
    public static async Task<Result<PromoCode>> ResolveAsync(
        IApplicationDbContext context,
        string code,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var normalized = PromoCode.NormalizeCode(code);

        var promo = await context.FirstOrDefaultAsync(
            context.PromoCodes.Where(p => p.Code == normalized),
            cancellationToken);

        if (promo is null)
            return Result<PromoCode>.Failure(PromoCodeErrors.NotFound);

        var usable = promo.CheckUsable(DateTime.UtcNow);
        if (usable.IsFailure)
            return Result<PromoCode>.Failure(usable.Errors);

        var alreadyUsed = await context.AnyAsync(
            context.PromoRedemptions.Where(r => r.PromoCodeId == promo.Id && r.UserId == userId),
            cancellationToken);

        if (alreadyUsed)
            return Result<PromoCode>.Failure(PromoCodeErrors.AlreadyUsed);

        return Result<PromoCode>.Success(promo);
    }
}
