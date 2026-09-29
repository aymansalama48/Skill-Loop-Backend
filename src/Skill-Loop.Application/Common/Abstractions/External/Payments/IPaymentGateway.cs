using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Abstractions.External.Payments;

/// <summary>
/// عقد "خد فلوس من المستخدم". الـ Application ما تعرفش Stripe ولا Paymob،
/// لما نختار بوابة حقيقية بنكتب تنفيذ جديد للـ Interface ده من غير ما نغيّر أي Handler.
/// </summary>
public interface IPaymentGateway
{
    Task<Result<PaymentSession>> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
}

/// <param name="AmountMinor">المبلغ بأصغر وحدة عملة (قرش/سنت).</param>
public sealed record PaymentRequest(Guid PurchaseId, Guid UserId, int AmountMinor, string Currency, string Description);

/// <param name="GatewayReference">رقم العملية عند البوابة.</param>
/// <param name="CheckoutUrl">لو البوابة بتحوّل المستخدم لصفحة دفع، ده رابطها. غير كده null.</param>
/// <param name="IsCompleted">true = الدفع تم فوراً. false = مستنيين تأكيد (Webhook) من البوابة.</param>
public sealed record PaymentSession(string GatewayReference, string? CheckoutUrl, bool IsCompleted);
