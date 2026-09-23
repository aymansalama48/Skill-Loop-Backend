using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.SiteSettings.Commands.UpdateSiteSettings;
using Skill_Loop.Application.Features.SiteSettings.Queries.GetSiteSettings;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Api.Controllers;

public class SiteSettingsController : BaseApiController
{
    /// <summary>
    /// جلب إعدادات الموقع الحالية (متاحة للجميع بدون صلاحيات ليستخدمها الـ Frontend)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSiteSettingsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// تحديث إعدادات الموقع (متاحة فقط للسوبر آدمن)
    /// </summary>
    [HttpPut]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<IResult> UpdateSettings(
        [FromBody] UpdateSiteSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}