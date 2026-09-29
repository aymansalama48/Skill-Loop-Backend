using FluentValidation;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionsPaged;

namespace Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionsPaged;

public sealed class GetSupportQuestionsPagedQueryValidator : AbstractValidator<GetSupportQuestionsPagedQuery>
{
    public GetSupportQuestionsPagedQueryValidator()
    {
        RuleFor(x => x.Pagination)
            .NotNull()
            .WithMessage("Pagination parameters are required.");

        When(x => x.Pagination != null, () =>
        {
            RuleFor(x => x.Pagination!.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.Pagination!.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be between 1 and 100.");
        });
    }
}