namespace Skill_Loop.Domain.Enums;

/// <summary>
/// نوع الخصم في كود الخصم: نسبة مئوية (مثلاً 20%) أو مبلغ ثابت (مثلاً 50 جنيه).
/// </summary>
public enum DiscountType
{
    Percentage = 1,
    FixedAmount = 2
}
