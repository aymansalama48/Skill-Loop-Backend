using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Bookings.Commands.ChangeBookingStatus;

[AuthenticatedOnly]
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
