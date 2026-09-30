using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.User;

public static class UserErrors
{
    public static readonly Error Unauthorized = new Error(
        "User.Unauthorized",
        "User unauthorized.",
        ErrorType.Unauthorized);

}
