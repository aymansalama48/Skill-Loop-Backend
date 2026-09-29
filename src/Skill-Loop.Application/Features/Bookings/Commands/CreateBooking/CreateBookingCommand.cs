using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;

public sealed record CreateBookingCommand(
    Guid SessionId,
    Guid LearnerUserId) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IReadOnlyCollection<string> CacheKeys =>
    [
        "sessions:all",
        $"sessions:{SessionId}"
    ];
}
