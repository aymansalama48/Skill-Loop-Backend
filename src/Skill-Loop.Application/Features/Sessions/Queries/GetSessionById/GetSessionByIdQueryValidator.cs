using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionById;

public sealed class GetSessionByIdQueryValidator : AbstractValidator<GetSessionByIdQuery>
{
    public GetSessionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");
    }
}