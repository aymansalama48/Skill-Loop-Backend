using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Sessions;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Commands.ChangeSessionStatus;
using Skill_Loop.Application.Features.Sessions.Commands.CreateSession;
using Skill_Loop.Application.Features.Sessions.Commands.DeleteSession;
using Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;
using Skill_Loop.Application.Features.Sessions.Queries.GetMySessionsPaged;
using Skill_Loop.Application.Features.Sessions.Queries.GetSessionById;
using Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;
using Skill_Loop.Domain.Enums;
namespace Skill_Loop.Api.Controllers;
/// <summary>
/// إدارة الجلسات المباشرة (الجدولة والنشر)
/// </summary>
[Route("api/v1/[controller]")]
[Authorize] // حماية الكنترولر بالكامل عشان محدش ينشئ جلسة من غير توكن
public class SessionsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    // حقن ICurrentUser
    public SessionsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }
    [HttpPost]
    public async Task<IResult> CreateSession(
            [FromBody] CreateSessionRequest request,
            CancellationToken cancellationToken)
    {
        // 1. سحب الـ ID الخاص بالمستخدم الحالي من التوكن تلقائياً
        var instructorId = RequireUserId();
        // 2. تمريره للـ Command
        var command = new CreateSessionCommand(
            request.Title,
            request.InstructorId ?? instructorId,
            request.Description,
            request.ScheduledAtUtc,
            request.DurationMinutes,
            request.CreditsPrice,
            request.LocationType,
            request.LocationDetails,
            request.LiveSessionUrl,
            request.MaxParticipants);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateSession(
        Guid id,
        [FromBody] UpdateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSessionCommand(
            id,
            request.Title,
            request.Description,
            request.ScheduledAtUtc,
            request.DurationMinutes,
            request.CreditsPrice,
            request.LocationType,
            request.LocationDetails,
            request.LiveSessionUrl,
            request.MaxParticipants);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteSession(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteSessionCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPatch("{id:guid}/status")]
    public async Task<IResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeSessionStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeSessionStatusCommand(id, request.Status);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetSessionById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSessionByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet]
    public async Task<IResult> GetSessionsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? instructorId = null,
        [FromQuery] SessionStatus? status = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] bool bookableOnly = false,
        [FromQuery] int? maxCredits = null,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationParameters { PageNumber = pageNumber, PageSize = pageSize };
        var query = new GetSessionsPagedQuery(
            pagination,
            instructorId,
            status,
            fromUtc,
            toUtc,
            bookableOnly,
            maxCredits);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
    [HttpGet("me")]
    public async Task<IResult> GetMySessions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] SessionStatus? status = null,
        [FromQuery] bool upcomingOnly = false,
        [FromQuery] bool pastOnly = false,
        CancellationToken cancellationToken = default)
    {
        var instructorId = RequireUserId();
        var query = new GetMySessionsPagedQuery(instructorId, pageNumber, pageSize, status, upcomingOnly, pastOnly);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
