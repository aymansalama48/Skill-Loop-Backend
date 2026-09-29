using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionById;

public sealed class GetSessionByIdQueryValidator : AbstractValidator<GetSessionByIdQuery>
{
    public GetSessionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف الجلسة مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف الجلسة غير صالح.");
    }
}