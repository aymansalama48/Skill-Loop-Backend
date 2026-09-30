using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Bookings.Shared;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Bookings.Queries.GetBookingById;

[AuthenticatedOnly]
public sealed record GetBookingByIdQuery(Guid BookingId) : IQuery<BookingResponse>;
