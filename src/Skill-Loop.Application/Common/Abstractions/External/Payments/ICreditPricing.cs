namespace Skill_Loop.Application.Common.Abstractions.External.Payments;

/// <summary>
/// سعر الكريديت الواحد والعملة. مقروء من الإعدادات (appsettings: Payments).
/// </summary>
public interface ICreditPricing
{
    /// <summary>سعر الكريديت الواحد بأصغر وحدة عملة (100 = جنيه واحد).</summary>
    int PricePerCreditMinor { get; }

    string Currency { get; }
}
