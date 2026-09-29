using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Common;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Wallets.Queries.GetMyWallet;
using Skill_Loop.Application.Features.Wallets.Queries.GetMyWalletTransactionsPaged;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/[controller]")]
[Authorize] // المحفظة دائماً محمية وتخص المستخدم الحالي
public class WalletsController : BaseApiController
{
    /// <summary>
    /// عرض رصيد المحفظة للمستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    public async Task<IResult> GetMyWallet(CancellationToken cancellationToken)
    {
        var query = new GetMyWalletQuery();
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// عرض سجل الحركات المالية (مع دعم الصفحات)
    /// </summary>
    [HttpGet("me/transactions")]
    public async Task<IResult> GetMyWalletTransactions(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetMyWalletTransactionsPagedQuery(request.PageNumber, request.PageSize);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}