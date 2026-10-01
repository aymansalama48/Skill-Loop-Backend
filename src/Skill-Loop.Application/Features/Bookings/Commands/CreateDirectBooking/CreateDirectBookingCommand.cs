using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateDirectBooking;

/// <summary>
/// أمر إنشاء حجز مباشر (Calendly-style). يقوم بإنشاء جلسة 1-on-1 خلف الكواليس ثم يحجزها للمتعلم.
/// </summary>
[AuthenticatedOnly]
public sealed record CreateDirectBookingCommand(
    Guid LearnerUserId,
    Guid InstructorId,
    DateTime ScheduledAtUtc,
    int DurationMinutes) : ICommand<Guid>;
