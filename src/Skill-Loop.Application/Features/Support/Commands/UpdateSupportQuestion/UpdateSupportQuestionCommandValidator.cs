using FluentValidation;
using Skill_Loop.Application.Features.Support.Commands.UpdateSupportQuestion;

namespace Skill_Loop.Application.Features.Support.Commands.UpdateSupportQuestion;

public sealed class UpdateSupportQuestionCommandValidator : AbstractValidator<UpdateSupportQuestionCommand>
{
    public UpdateSupportQuestionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Question id is required.");

        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage("Question is required.")
            .MaximumLength(2000)
            .WithMessage("Question cannot exceed 2000 characters.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required.")
            .MaximumLength(100)
            .WithMessage("Category cannot exceed 100 characters.");

        RuleFor(x => x.Answer)
            .MaximumLength(5000)
            .WithMessage("Answer cannot exceed 5000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Answer));

        RuleFor(x => x.IsPublished)
            .Equal(true)
            .When(x => !string.IsNullOrEmpty(x.Answer))
            .WithMessage("Cannot publish without an answer.")
            .OverridePropertyName("IsPublished");
    }
}
