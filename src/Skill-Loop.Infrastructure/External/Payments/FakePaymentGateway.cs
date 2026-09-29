using Microsoft.Extensions.Logging;
using Skill_Loop.Application.Common.Abstractions.External.Payments;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Infrastructure.External.Payments;

/// <summary>
/// ⚠️ بوابة دفع تجريبية: بترد "نجح" على أي طلب من غير ما تاخد فلوس فعلاً.
/// للتطوير والتجربة بس. الـ DI بيرفض تشغيلها في Production.
/// لما نختار Stripe/Paymob، بنكتب Class جديد بينفّذ IPaymentGateway ونبدّل التسجيل.
/// </summary>
internal sealed class FakePaymentGateway(ILogger<FakePaymentGateway> logger) : IPaymentGateway
{
    public Task<Result<PaymentSession>> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "[FakePaymentGateway] دفع تجريبي بدون فلوس حقيقية: {Amount} {Currency} للمستخدم {UserId}",
            request.AmountMinor, request.Currency, request.UserId);

        return Task.FromResult(Result<PaymentSession>.Success(
            new PaymentSession($"fake_{Guid.NewGuid():N}", CheckoutUrl: null, IsCompleted: true)));
    }
}
