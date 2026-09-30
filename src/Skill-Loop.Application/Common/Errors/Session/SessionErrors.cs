using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Session;

public static class SessionErrors
{
    public static readonly Error NotFound = new Error(
        "Session.NotFound",
        "Session was not found.",
        ErrorType.NotFound);

    public static readonly Error HasMaterials = new Error(
        "Session.HasMaterials",
        "Session has materials.",
        ErrorType.Conflict);

    public static readonly Error HasActiveBookings = new Error(
        "Session.HasActiveBookings",
        "Session has active bookings.",
        ErrorType.Conflict);

    public static readonly Error Forbidden = new Error(
        "Session.Forbidden",
        "You do not have permission to access or modify this session.",
        ErrorType.Forbidden);

}
