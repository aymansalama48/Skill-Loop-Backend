using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Bookings;

public static class BookingErrors
{
    public static readonly Error NotFound = new(
        "BOOKING_NOT_FOUND",
        "Booking not found.",
        ErrorType.NotFound);

    public static readonly Error NotLearner = new(
        "BOOKING_NOT_LEARNER",
        "Only the learner who owns this booking can modify it.",
        ErrorType.Forbidden);

    public static readonly Error NotSessionInstructor = new(
        "BOOKING_NOT_SESSION_INSTRUCTOR",
        "Only the instructor of this session can modify this booking.",
        ErrorType.Forbidden);

    public static readonly Error SessionNotFound = new(
        "BOOKING_SESSION_NOT_FOUND",
        "The session you are trying to book does not exist.",
        ErrorType.NotFound);

    public static readonly Error SessionNotPublished = new(
        "BOOKING_SESSION_NOT_PUBLISHED",
        "Only published sessions can be booked.",
        ErrorType.Conflict);

    public static readonly Error SessionInThePast = new(
        "BOOKING_SESSION_IN_THE_PAST",
        "You cannot book a session that has already started.",
        ErrorType.Conflict);

    public static readonly Error SessionNotScheduled = new(
        "BOOKING_SESSION_NOT_SCHEDULED",
        "This session has no scheduled date yet.",
        ErrorType.Conflict);

    public static readonly Error SessionFull = new(
        "BOOKING_SESSION_FULL",
        "The session has no available slots left.",
        ErrorType.Conflict);

    public static readonly Error SelfBooking = new(
        "BOOKING_SELF_BOOKING",
        "You cannot book your own session.",
        ErrorType.Validation);

    public static readonly Error AlreadyBooked = new(
        "BOOKING_ALREADY_BOOKED",
        "You already have an active booking for this session.",
        ErrorType.Conflict);

    public static readonly Error WalletNotFound = new(
        "BOOKING_WALLET_NOT_FOUND",
        "Your virtual wallet was not found.",
        ErrorType.NotFound);

    public static readonly Error CannotCancel = new(
        "BOOKING_CANNOT_CANCEL",
        "This booking can no longer be cancelled.",
        ErrorType.Conflict);

    public static readonly Error InvalidStatusChange = new(
        "BOOKING_INVALID_STATUS_CHANGE",
        "The requested status change is not allowed from the current status.",
        ErrorType.Conflict);

    public static readonly Error StatusRequired = new(
        "BOOKING_STATUS_REQUIRED",
        "Booking status is required.",
        ErrorType.Validation);
}
