using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Bookings.Commands.CompleteBooking;

public sealed record CompleteBookingCommand(Guid BookingId) : ICommand<bool>;