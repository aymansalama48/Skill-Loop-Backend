using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;

public sealed class GetSessionsPagedQueryValidator : AbstractValidator<GetSessionsPagedQuery>
{
    public GetSessionsPagedQueryValidator()
    {
        RuleFor(x => x.Pagination)
            .NotNull().WithMessage("إعدادات الصفحات مطلوبة.");

        When(x => x.Pagination != null, () =>
        {
            RuleFor(x => x.Pagination.PageNumber)
                .GreaterThan(0).WithMessage("رقم الصفحة يجب أن يكون أكبر من الصفر.");

            RuleFor(x => x.Pagination.PageSize)
                .GreaterThan(0).WithMessage("حجم الصفحة يجب أن يكون أكبر من الصفر.")
                .LessThanOrEqualTo(100).WithMessage("حجم الصفحة لا يمكن أن يتجاوز 100.");
        });
    }
}