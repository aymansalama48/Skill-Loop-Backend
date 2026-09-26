using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;

public sealed record CreateBookingCommand(
    Guid SessionId,
    DateTime ScheduleDate,
    TimeSpan StartTime
) : ICommand<Guid>;