using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCoursesPaged;

public sealed class GetCoursesPagedQueryValidator : AbstractValidator<GetCoursesPagedQuery>
{
    public GetCoursesPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.MinRating).InclusiveBetween(0, 5).When(x => x.MinRating.HasValue);
        RuleFor(x => x.MaxCredits).GreaterThanOrEqualTo(0).When(x => x.MaxCredits.HasValue);
    }
}