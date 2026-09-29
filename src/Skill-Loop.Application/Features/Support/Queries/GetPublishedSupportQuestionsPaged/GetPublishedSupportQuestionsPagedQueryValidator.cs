using FluentValidation;
using Skill_Loop.Application.Features.Support.Queries.GetPublishedSupportQuestionsPaged;

namespace Skill_Loop.Application.Features.Support.Queries.GetPublishedSupportQuestionsPaged;

public sealed class GetPublishedSupportQuestionsPagedQueryValidator : AbstractValidator<GetPublishedSupportQuestionsPagedQuery>
{
    public GetPublishedSupportQuestionsPagedQueryValidator()
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
