using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Payments;

public static class PaymentErrors
{
    public static readonly Error GatewayFailed = new(
        "PAYMENT_GATEWAY_FAILED", "تعذّر إتمام عملية الدفع. حاول مرة أخرى.", ErrorType.Failure);
}
