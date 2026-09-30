using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.SiteSettings.Commands.UpdateSiteSettings;
using Skill_Loop.Application.Features.SiteSettings.Queries.GetSiteSettings;
using Skill_Loop.Domain.Constants;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة إعدادات الموقع العامة والتكوينات
/// </summary>
[Route("api/v1/[controller]")]
public class SiteSettingsController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSiteSettingsQuery(), cancellationToken);
        return HandleResult(result);
    }
    [HttpPut]
    [Authorize]
    public async Task<IResult> UpdateSettings(
        [FromBody] UpdateSiteSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
