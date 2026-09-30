using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;

[AuthenticatedOnly]
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
