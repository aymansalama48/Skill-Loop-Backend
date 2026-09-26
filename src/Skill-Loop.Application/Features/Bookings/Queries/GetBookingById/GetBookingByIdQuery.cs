using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Bookings.Shared;

namespace Skill_Loop.Application.Features.Bookings.Queries.GetBookingById;

public sealed record GetBookingByIdQuery(Guid BookingId) : IQuery<BookingResponse>;
