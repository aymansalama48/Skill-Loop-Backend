using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Common;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Wallets.Queries.GetMyWallet;
using Skill_Loop.Application.Features.Wallets.Queries.GetMyWalletTransactionsPaged;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة المحافظ المالية والرصيد
/// </summary>
[Route("api/v1/[controller]")]
[Authorize] // المحفظة دائماً محمية وتخص المستخدم الحالي
public class WalletsController : BaseApiController
{
    [HttpGet("me")]
    public async Task<IResult> GetMyWallet(CancellationToken cancellationToken)
    {
        var query = new GetMyWalletQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("me/transactions")]
    public async Task<IResult> GetMyWalletTransactions(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetMyWalletTransactionsPagedQuery(request.PageNumber, request.PageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("me/earnings")]
    public async Task<IResult> GetMyEarningsSummary(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var query = new Skill_Loop.Application.Features.Wallets.Queries.GetMyEarningsSummary.GetMyEarningsSummaryQuery(year, month);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}