using FluentValidation;
using Skill_Loop.Application.Features.Support.Commands.AnswerSupportQuestion;

namespace Skill_Loop.Application.Features.Support.Commands.AnswerSupportQuestion;

public sealed class AnswerSupportQuestionCommandValidator : AbstractValidator<AnswerSupportQuestionCommand>
{
    public AnswerSupportQuestionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Question id is required.");

        RuleFor(x => x.Answer)
            .NotEmpty()
            .WithMessage("Answer is required.")
            .MaximumLength(5000)
            .WithMessage("Answer cannot exceed 5000 characters.");
    }
}
