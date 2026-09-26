using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;

public sealed record CancelBookingCommand(Guid BookingId) : ICommand<bool>;