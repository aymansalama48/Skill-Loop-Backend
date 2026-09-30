using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;

public sealed class GetSessionsPagedQueryValidator : AbstractValidator<GetSessionsPagedQuery>
{
    public GetSessionsPagedQueryValidator()
    {
        RuleFor(x => x.Pagination)
            .NotNull().WithMessage("This field is required.");

        When(x => x.Pagination != null, () =>
        {
            RuleFor(x => x.Pagination.PageNumber)
                .GreaterThan(0).WithMessage("Value must be greater than 0.");

            RuleFor(x => x.Pagination.PageSize)
                .GreaterThan(0).WithMessage("Value must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Length exceeds the maximum allowed.");
        });
    }
}