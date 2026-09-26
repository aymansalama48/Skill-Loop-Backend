using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;

public sealed record CancelBookingCommand(
    Guid BookingId,
    Guid RequestedByUserId,
    string? Reason = null) : ICommand, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "sessions:all",
        $"bookings:{BookingId}"
    ];
}
