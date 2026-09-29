using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Bookings.Commands.ChangeBookingStatus;

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
