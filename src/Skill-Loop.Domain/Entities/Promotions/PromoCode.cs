using System.Text.RegularExpressions;
using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Promotions;

/// <summary>
/// كود خصم بيتطبق على سعر شراء الكريديت.
/// - DiscountType.Percentage: DiscountValue = نسبة من 1 لـ 100.
/// - DiscountType.FixedAmount: DiscountValue = مبلغ ثابت بأصغر وحدة عملة (قرش/سنت).
/// </summary>
public sealed class PromoCode : AuditableEntity
{
    private static readonly Regex CodeFormat = new("^[A-Z0-9_-]{3,50}$", RegexOptions.Compiled);

    public string Code { get; private set; } = string.Empty;
    public DiscountType DiscountType { get; private set; }
    public int DiscountValue { get; private set; }

    /// <summary>أقصى عدد مرات استخدام في المجموع. null = بلا حد.</summary>
    public int? MaxRedemptions { get; private set; }
    public int RedemptionsCount { get; private set; }

    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Optimistic Concurrency: لو اتنين استخدموا آخر استخدام متاح في نفس اللحظة،
    /// واحد بس هينجح والتاني هياخد Exception بدل ما نعدّي الحد.
    /// </summary>
    public byte[] RowVersion { get; private set; } = [];

    private PromoCode() { }

    public static string NormalizeCode(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();

    public static Result<PromoCode> Create(
        string? code,
        DiscountType discountType,
        int discountValue,
        int? maxRedemptions,
        DateTime? expiresAt)
    {
        var normalized = NormalizeCode(code);

        if (!CodeFormat.IsMatch(normalized))
            return Result<PromoCode>.Failure(new Error(
                "Promo.InvalidCode",
                "الكود لازم يكون من 3 لـ 50 حرف (حروف إنجليزي وأرقام و - و _ فقط).",
                ErrorType.Validation));

        if (discountType == DiscountType.Percentage && discountValue is < 1 or > 100)
            return Result<PromoCode>.Failure(new Error(
                "Promo.InvalidPercentage", "النسبة لازم تكون بين 1 و 100.", ErrorType.Validation));

        if (discountType == DiscountType.FixedAmount && discountValue < 1)
            return Result<PromoCode>.Failure(new Error(
                "Promo.InvalidAmount", "قيمة الخصم لازم تكون أكبر من صفر.", ErrorType.Validation));

        if (maxRedemptions is < 1)
            return Result<PromoCode>.Failure(new Error(
                "Promo.InvalidMaxRedemptions", "أقصى عدد استخدامات لازم يكون 1 أو أكتر.", ErrorType.Validation));

        if (expiresAt is not null && expiresAt <= DateTime.UtcNow)
            return Result<PromoCode>.Failure(new Error(
                "Promo.InvalidExpiry", "تاريخ الانتهاء لازم يكون في المستقبل.", ErrorType.Validation));

        return Result<PromoCode>.Success(new PromoCode
        {
            Code = normalized,
            DiscountType = discountType,
            DiscountValue = discountValue,
            MaxRedemptions = maxRedemptions,
            ExpiresAt = expiresAt,
            IsActive = true
        });
    }

    /// <summary>هل الكود ينفع يتستخدم دلوقتي (مفعّل، مش منتهي، لسه فيه استخدامات)؟</summary>
    public Result CheckUsable(DateTime nowUtc)
    {
        if (!IsActive)
            return Result.Failure(new Error("Promo.Inactive", "كود الخصم غير مفعّل.", ErrorType.Validation));

        if (ExpiresAt is not null && ExpiresAt <= nowUtc)
            return Result.Failure(new Error("Promo.Expired", "كود الخصم منتهي الصلاحية.", ErrorType.Validation));

        if (MaxRedemptions is not null && RedemptionsCount >= MaxRedemptions)
            return Result.Failure(new Error("Promo.Exhausted", "كود الخصم وصل للحد الأقصى من الاستخدامات.", ErrorType.Validation));

        return Result.Success();
    }

    /// <summary>قيمة الخصم على مبلغ معين (بأصغر وحدة عملة). عمرها ما بتزيد عن المبلغ نفسه.</summary>
    public int CalculateDiscount(int subtotalMinor)
    {
        if (subtotalMinor <= 0) return 0;

        var discount = DiscountType == DiscountType.Percentage
            ? (int)((long)subtotalMinor * DiscountValue / 100)
            : DiscountValue;

        return Math.Min(discount, subtotalMinor);
    }

    /// <summary>بتتنادى مرة لما دفع مستخدم بالكود ينجح.</summary>
    public Result Redeem(DateTime nowUtc)
    {
        var usable = CheckUsable(nowUtc);
        if (usable.IsFailure) return usable;

        RedemptionsCount++;
        return Result.Success();
    }

    public void Deactivate() => IsActive = false;
}
