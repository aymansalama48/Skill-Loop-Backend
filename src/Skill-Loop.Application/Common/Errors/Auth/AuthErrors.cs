using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Auth;

public static class AuthErrors
{
    public static readonly Error Unauthorized = new Error(
        "Auth.Unauthorized",
        "Auth unauthorized.",
        ErrorType.Unauthorized);

    public static readonly Error MissingIdentifier = new Error(
        "Auth.MissingIdentifier",
        "Auth missing identifier.",
        ErrorType.Unauthorized);

    public static readonly Error Forbidden = new Error(
        "Auth.Forbidden",
        "You do not have permission to modify this auth.",
        ErrorType.Forbidden);

}
