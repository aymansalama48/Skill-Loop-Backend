using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Dashboards.Queries.GetAdminDashboardSummary;
using Skill_Loop.Application.Features.Dashboards.Queries.GetInstructorDashboardSummary;
using Skill_Loop.Application.Features.Dashboards.Queries.GetStudentDashboardSummary;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Api.Controllers;

/// <summary>
/// لوحة التحكم والإحصائيات الخاصة بالمستخدمين بمختلف أدوارهم
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardsController : BaseApiController
{
    /// <summary>
    /// إحصائيات لوحة تحكم الإدارة
    /// </summary>
    [HttpGet("admin")]
    public async Task<IResult> GetAdminSummary(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAdminDashboardSummaryQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إحصائيات لوحة تحكم المدرب
    /// </summary>
    [HttpGet("instructor")]
    public async Task<IResult> GetInstructorSummary(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetInstructorDashboardSummaryQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// إحصائيات لوحة تحكم الطالب
    /// </summary>
    [HttpGet("student")]
    public async Task<IResult> GetStudentSummary(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetStudentDashboardSummaryQuery(), cancellationToken);
        return HandleResult(result);
    }
}
