using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Bookings.Commands.ChangeBookingStatus;

// Sessions.Moderate, not Bookings.ManageAll: the handler refuses anyone who is not the
// session's own instructor or owner, so the permission only has to express "may move a
// session through its states". Requiring the global Bookings.ManageAll here meant an
// instructor could never confirm a booking against their own session - the request was
// rejected before the ownership check ever ran.
[Permission(Permissions.Sessions.Moderate)]
public sealed record ChangeBookingStatusCommand(
    Guid BookingId,
    Guid InstructorUserId,
    BookingStatus NewStatus,
    string? Reason = null) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "sessions:all",
        $"bookings:{BookingId}"
    ];
}
