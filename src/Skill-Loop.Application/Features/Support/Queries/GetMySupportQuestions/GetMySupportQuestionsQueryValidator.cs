using FluentValidation;
using Skill_Loop.Application.Features.Support.Queries.GetMySupportQuestions;

namespace Skill_Loop.Application.Features.Support.Queries.GetMySupportQuestions;

public sealed class GetMySupportQuestionsQueryValidator : AbstractValidator<GetMySupportQuestionsQuery>
{
    public GetMySupportQuestionsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User id is required.");

        RuleFor(x => x.Pagination)
            .NotNull()
            .WithMessage("Pagination parameters are required.");
    }
}
