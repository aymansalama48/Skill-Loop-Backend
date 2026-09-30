using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.PromoCodes.Commands.CreatePromoCode;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Api.Controllers.v1.Admin;

[Route("api/v1/Admin/PromoCodes")]
[Authorize(Roles = Roles.SuperAdmin)]
public class AdminPromoCodesController : BaseApiController
{
    [HttpPost]
    public async Task<IResult> CreatePromoCode([FromBody] CreatePromoCodeCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
