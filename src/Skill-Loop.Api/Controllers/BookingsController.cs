using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Bookings;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;
using Skill_Loop.Application.Features.Bookings.Commands.CompleteBooking;
using Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;

namespace Skill_Loop.Api.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
public class BookingsController : BaseApiController
{
    [HttpPost]
    public async Task<IResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBookingCommand(
            request.SessionId,
            request.ScheduleDate,
            request.StartTime
        );

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IResult> CompleteBooking(Guid id, CancellationToken cancellationToken)
    {
        var command = new CompleteBookingCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IResult> CancelBooking(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelBookingCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}