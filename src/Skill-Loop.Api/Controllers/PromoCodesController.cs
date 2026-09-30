using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Payments;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Errors.Promotions;
using Skill_Loop.Application.Features.Promotions.Commands.CreatePromoCode;
using Skill_Loop.Application.Features.Promotions.Commands.DeactivatePromoCode;
using Skill_Loop.Application.Features.Promotions.Queries.GetPromoCodes;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Constants;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة كوبونات الخصم والرموز الترويجية
/// </summary>
[Route("api/v1/promo-codes")]
[Authorize]
public class PromoCodesController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    public PromoCodesController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    private bool CanManage => _currentUser.HasPermission(Permissions.Finance.PromoCodesManage);
    [HttpPost]
    public async Task<IResult> Create([FromBody] CreatePromoCodeRequest request, CancellationToken cancellationToken)
    {
        if (!CanManage)
            return HandleResult(Result.Failure(PromoCodeErrors.Forbidden));
        var command = new CreatePromoCodeCommand(
            request.Code, request.DiscountType, request.DiscountValue, request.MaxRedemptions, request.ExpiresAt);
        return HandleResult(await Mediator.Send(command, cancellationToken));
    }
    [HttpGet]
    public async Task<IResult> GetAll([FromQuery] GetPromoCodesRequest request, CancellationToken cancellationToken)
    {
        if (!CanManage)
            return HandleResult(Result.Failure(PromoCodeErrors.Forbidden));
        var query = new GetPromoCodesQuery { PageNumber = request.PageNumber, PageSize = request.PageSize };
        return HandleResult(await Mediator.Send(query, cancellationToken));
    }
    [HttpPost("{promoCodeId:guid}/deactivate")]
    public async Task<IResult> Deactivate(Guid promoCodeId, CancellationToken cancellationToken)
    {
        if (!CanManage)
            return HandleResult(Result.Failure(PromoCodeErrors.Forbidden));
        return HandleResult(await Mediator.Send(new DeactivatePromoCodeCommand(promoCodeId), cancellationToken));
    }
    [HttpGet("{code}/validate")]
    [AllowAnonymous] // Anyone should be able to validate a promo code
    public async Task<IResult> Validate(string code, CancellationToken cancellationToken)
    {
        return HandleResult(await Mediator.Send(new Skill_Loop.Application.Features.Promotions.Queries.ValidatePromoCode.ValidatePromoCodeQuery(code), cancellationToken));
    }
}
