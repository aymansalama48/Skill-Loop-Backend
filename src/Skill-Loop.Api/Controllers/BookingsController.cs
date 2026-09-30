using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skill_Loop.Api.Contracts.Bookings;
using Skill_Loop.Api.Contracts.Common;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Errors.Bookings;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;
using Skill_Loop.Application.Features.Bookings.Commands.ChangeBookingStatus;
using Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;
using Skill_Loop.Application.Features.Bookings.Queries.GetBookingById;
using Skill_Loop.Application.Features.Bookings.Queries.GetMyBookings;
using Skill_Loop.Application.Features.Bookings.Queries.GetSessionBookings;
using Skill_Loop.Application.Features.Bookings.Shared;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Controllers;

/// <summary>
/// حجز الجلسات لايف (Live Session Booking)
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
public class BookingsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public BookingsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// حجز جلسة لايف — يخصم الـ credits من محفظة المستخدم الحالي
    /// </summary>
    [HttpPost]
    public async Task<IResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } learnerId || learnerId == Guid.Empty)
        {
            return HandleResult(Result<Guid>.Failure(BookingErrors.NotLearner));
        }

        var command = new CreateBookingCommand(request.SessionId, learnerId);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// إلغاء حجز (المتعلم أو المحاضر) — بيسترجع الـ credits
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IResult> CancelBooking(
        Guid id,
        [FromBody] CancelBookingRequest? request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId || userId == Guid.Empty)
        {
            return HandleResult(Result.Failure(BookingErrors.NotLearner));
        }

        var command = new CancelBookingCommand(id, userId, request?.Reason);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// تغيير حالة الحجز (المحاضر فقط): InProgress / Completed / Rejected / NoShow
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IResult> ChangeBookingStatus(
        Guid id,
        [FromQuery] BookingStatus status,
        [FromBody] ChangeBookingStatusRequest? request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } instructorId || instructorId == Guid.Empty)
        {
            return HandleResult(Result.Failure(BookingErrors.NotSessionInstructor));
        }

        var command = new ChangeBookingStatusCommand(id, instructorId, status, request?.Reason);
        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// كل حجوزاتي (كمتعلم)
    /// </summary>
    [HttpGet("me")]
    public async Task<IResult> GetMyBookings(
        [FromQuery] PaginationRequest request,
        [FromQuery] BookingStatus? status = null,
        [FromQuery] bool upcomingOnly = false,
        [FromQuery] bool pastOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } userId || userId == Guid.Empty)
        {
            return HandleResult(Result<PagedResult<BookingResponse>>.Failure(BookingErrors.NotLearner));
        }

        var query = new GetMyBookingsQuery(userId, request.PageNumber, request.PageSize, status, upcomingOnly, pastOnly);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }



    /// <summary>
    /// تفاصيل حجز واحد
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetBookingById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetBookingByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}
