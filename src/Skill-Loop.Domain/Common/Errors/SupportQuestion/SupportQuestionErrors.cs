using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Domain.Common.Errors.SupportQuestion;

public static class SupportQuestionErrors
{
    public static readonly Error EmptyQuestion = new Error(
        "SupportQuestion.EmptyQuestion",
        "SupportQuestion is required.",
        ErrorType.Validation);

    public static readonly Error EmptyCategory = new Error(
        "SupportQuestion.EmptyCategory",
        "SupportQuestion is required.",
        ErrorType.Validation);

    public static readonly Error EmptyAnswer = new Error(
        "SupportQuestion.EmptyAnswer",
        "SupportQuestion empty answer.",
        ErrorType.Validation);

    public static readonly Error NoAnswer = new Error(
        "SupportQuestion.NoAnswer",
        "SupportQuestion no answer.",
        ErrorType.Validation);

}
