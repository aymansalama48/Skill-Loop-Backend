using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.SupportQuestion;

public static class SupportQuestionErrors
{
    public static readonly Error MissingIdentifier = new Error(
        "SupportQuestion.MissingIdentifier",
        "SupportQuestion missing identifier.",
        ErrorType.Unauthorized);

    public static readonly Error NotFound = new Error(
        "SupportQuestion.NotFound",
        "SupportQuestion was not found.",
        ErrorType.NotFound);

    public static readonly Error Unauthenticated = new Error(
        "SupportQuestion.Unauthenticated",
        "SupportQuestion unauthenticated.",
        ErrorType.Unauthorized);

    public static readonly Error AnswerNotAvailable = new Error(
        "SupportQuestion.AnswerNotAvailable",
        "SupportQuestion answer not available.",
        ErrorType.NotFound);

}
