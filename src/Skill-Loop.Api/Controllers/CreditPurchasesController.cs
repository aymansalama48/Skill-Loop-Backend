using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Payments;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.CreditPurchases.Commands.PurchaseCredits;
using Skill_Loop.Application.Features.CreditPurchases.Queries.GetPurchaseQuote;

namespace Skill_Loop.Api.Controllers;

/// <summary>شحن المحفظة بشراء كريديت (مع كود خصم اختياري).</summary>
[Route("api/v1/credit-purchases")]
[Authorize]
public class CreditPurchasesController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public CreditPurchasesController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// معاينة السعر قبل الدفع، بعد تطبيق كود الخصم (لو موجود). مفيش أي حاجة بتتحفظ.
    /// المبالغ بأصغر وحدة عملة (100 = جنيه واحد).
    /// </summary>
    [HttpPost("quote")]
    public async Task<IResult> Quote([FromBody] PurchaseQuoteRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var result = await Mediator.Send(new GetPurchaseQuoteQuery(userId, request.Credits, request.PromoCode), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>شراء كريديت. الرد فيه الحالة (Completed/Pending) والرصيد الجديد أو رابط الدفع.</summary>
    [HttpPost]
    public async Task<IResult> Purchase([FromBody] PurchaseCreditsRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var result = await Mediator.Send(new PurchaseCreditsCommand(userId, request.Credits, request.PromoCode), cancellationToken);
        return HandleResult(result);
    }
}
