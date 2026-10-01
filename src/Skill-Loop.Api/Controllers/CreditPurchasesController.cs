using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Payments;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.CreditPurchases.Commands.PurchaseCredits;
using Skill_Loop.Application.Features.CreditPurchases.Queries.GetPurchaseQuote;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة عمليات شراء الرصيد (Credits) وإضافتها للمحفظة
/// </summary>
[Route("api/v1/credit-purchases")]
[Authorize]
public class CreditPurchasesController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    public CreditPurchasesController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    [HttpPost("quote")]
    public async Task<IResult> Quote([FromBody] PurchaseQuoteRequest request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId(); // Ensures user is authenticated and ID is present
        var result = await Mediator.Send(new GetPurchaseQuoteQuery(request.Credits, request.PromoCode), cancellationToken);
        return HandleResult(result);
    }
    [HttpPost]
    public async Task<IResult> Purchase([FromBody] PurchaseCreditsRequest request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId(); // Ensures user is authenticated and ID is present
        var result = await Mediator.Send(new PurchaseCreditsCommand(request.Credits, request.PromoCode), cancellationToken);
        return HandleResult(result);
    }
}
