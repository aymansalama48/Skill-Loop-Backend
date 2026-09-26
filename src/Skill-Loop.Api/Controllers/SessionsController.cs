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
using Skill_Loop.Application.Features.Sessions.Queries.GetSessionById;
using Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/[controller]")]
[Authorize] // حماية الكنترولر بالكامل عشان محدش ينشئ جلسة من غير توكن
public class SessionsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

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
        var instructorId = _currentUser.UserId ?? Guid.Empty;

        // 2. تمريره للـ Command مع البيانات الجديدة
        var command = new CreateSessionCommand(
            request.Title,
            instructorId,
            request.PriceInCredits,
            request.DurationInMinutes,
            request.Type);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateSession(
        Guid id,
        [FromBody] UpdateSessionRequest request,
        CancellationToken cancellationToken)
    {
        // تمرير البيانات الجديدة للـ Command
        var command = new UpdateSessionCommand(
            id,
            request.Title,
            request.PriceInCredits,
            request.DurationInMinutes,
            request.Type);

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
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationParameters { PageNumber = pageNumber, PageSize = pageSize };
        var query = new GetSessionsPagedQuery(pagination, instructorId);

        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}