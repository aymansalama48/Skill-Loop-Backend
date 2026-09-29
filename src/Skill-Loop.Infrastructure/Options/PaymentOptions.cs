namespace Skill_Loop.Infrastructure.Options;

public sealed class PaymentOptions
{
    public const string SectionName = "Payments";

    /// <summary>البوابة المستخدمة. حالياً "Fake" بس (تجريبية). Stripe/Paymob لما نختار ونكتب تنفيذها.</summary>
    public string Gateway { get; set; } = "Fake";

    public string Currency { get; set; } = "EGP";

    /// <summary>سعر الكريديت الواحد بأصغر وحدة عملة (100 = جنيه واحد).</summary>
    public int PricePerCreditMinor { get; set; } = 100;
}
