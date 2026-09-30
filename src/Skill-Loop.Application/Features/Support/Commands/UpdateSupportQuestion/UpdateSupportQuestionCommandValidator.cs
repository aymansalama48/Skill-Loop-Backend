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

        // Publishing requires an answer; answering does not require publishing
        // (a drafted answer can be saved and published later).
        RuleFor(x => x.Answer)
            .NotEmpty()
            .When(x => x.IsPublished)
            .WithMessage("Cannot publish without an answer.")
            .OverridePropertyName("Answer");
    }
}
