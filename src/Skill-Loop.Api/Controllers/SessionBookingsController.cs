using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Common;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Bookings.Queries.GetSessionBookings;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/sessions/{sessionId:guid}/bookings")]
[Authorize]
[Tags("Session Bookings")]
public class SessionBookingsController : BaseApiController
{
    /// <summary>
    /// حجوزات جلسة معينة (للمحاضر)
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetSessionBookings(
        Guid sessionId,
        [FromQuery] PaginationRequest request,
        [FromQuery] BookingStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSessionBookingsQuery(sessionId, request.PageNumber, request.PageSize, status);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}
