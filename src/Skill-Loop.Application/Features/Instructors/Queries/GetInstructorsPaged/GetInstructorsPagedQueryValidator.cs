using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorsPaged;

public sealed class GetInstructorsPagedQueryValidator : AbstractValidator<GetInstructorsPagedQuery>
{
    public GetInstructorsPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("رقم الصفحة يجب أن يكون أكبر من الصفر.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("حجم الصفحة يجب أن يكون أكبر من الصفر.")
            .LessThanOrEqualTo(100).WithMessage("الحد الأقصى لحجم الصفحة هو 100.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5).WithMessage("التقييم يجب أن يكون بين 0 و 5 نجوم.")
            .When(x => x.MinRating.HasValue);
    }
}