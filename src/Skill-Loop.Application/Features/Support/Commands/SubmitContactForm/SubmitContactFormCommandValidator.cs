using FluentValidation;
using Skill_Loop.Application.Features.Support.Commands.SubmitContactForm;

namespace Skill_Loop.Application.Features.Support.Commands.SubmitContactForm;

public sealed class SubmitContactFormCommandValidator : AbstractValidator<SubmitContactFormCommand>
{
    public SubmitContactFormCommandValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Subject is required.")
            .MaximumLength(200)
            .WithMessage("Subject cannot exceed 200 characters.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(5000)
            .WithMessage("Message cannot exceed 5000 characters.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required.")
            .MaximumLength(100)
            .WithMessage("Category cannot exceed 100 characters.");
    }
}
