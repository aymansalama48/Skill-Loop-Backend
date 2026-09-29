using FluentValidation;
using Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionById;

namespace Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionById;

public sealed class GetSupportQuestionByIdQueryValidator : AbstractValidator<GetSupportQuestionByIdQuery>
{
    public GetSupportQuestionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Question ID is required.");
    }
}