using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorsPaged;

public sealed class GetInstructorsPagedQueryValidator : AbstractValidator<GetInstructorsPagedQuery>
{
    public GetInstructorsPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Value must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Value must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Invalid value.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5).WithMessage("Value is out of range.")
            .When(x => x.MinRating.HasValue);
    }
}