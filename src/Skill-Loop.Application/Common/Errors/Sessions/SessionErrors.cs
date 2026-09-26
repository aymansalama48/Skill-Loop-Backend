using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Sessions;

public static class SessionErrors
{
    public static readonly Error NotFound = new(
        "SESSION_NOT_FOUND",
        "Session not found.",
        ErrorType.NotFound);

    public static readonly Error NotOwner = new(
        "SESSION_NOT_OWNER",
        "Only the session owner can perform this operation.",
        ErrorType.Forbidden);

    public static readonly Error NotScheduled = new(
        "SESSION_NOT_SCHEDULED",
        "The session must have a scheduled date/time before it can be published or booked.",
        ErrorType.Validation);

    public static readonly Error NotPublished = new(
        "SESSION_NOT_PUBLISHED",
        "Only published sessions can be booked.",
        ErrorType.Conflict);

    public static readonly Error AlreadyFinished = new(
        "SESSION_ALREADY_FINISHED",
        "The session time has already passed.",
        ErrorType.Conflict);

    public static readonly Error FullyBooked = new(
        "SESSION_FULLY_BOOKED",
        "The session has no available slots left.",
        ErrorType.Conflict);

    public static readonly Error HasActiveBookings = new(
        "SESSION_HAS_ACTIVE_BOOKINGS",
        "You cannot delete a session that still has active bookings.",
        ErrorType.Conflict);

    public static readonly Error OwnerCannotChangeStatus = new(
        "SESSION_OWNER_CANNOT_CHANGE_STATUS",
        "Only the session owner can change the session status.",
        ErrorType.Forbidden);
}
