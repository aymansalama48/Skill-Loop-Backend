using Skill_Loop.Domain.Common.Events;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Booking;

/// <summary>
/// أخطاء الـ Domain الخاصة بالـ Booking (مستقلة عن طبقة الـ Application)
/// </summary>
public static class BookingDomainErrors
{
    public static readonly Error SessionIdRequired = new(
        "BOOKING_SESSION_ID_REQUIRED",
        "Session id is required.",
        ErrorType.Validation);

    public static readonly Error LearnerIdRequired = new(
        "BOOKING_LEARNER_ID_REQUIRED",
        "Learner user id is required.",
        ErrorType.Validation);

    public static readonly Error InvalidPrice = new(
        "BOOKING_INVALID_PRICE",
        "Booking price cannot be negative.",
        ErrorType.Validation);

    public static Error InvalidTransition(BookingStatus from, BookingStatus to) => new(
        "BOOKING_INVALID_STATUS_TRANSITION",
        $"Cannot change booking status from '{from}' to '{to}'.",
        ErrorType.Conflict);
}
