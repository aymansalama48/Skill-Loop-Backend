using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Commands.AddSection;

public sealed class AddSectionCommandValidator : AbstractValidator<AddSectionCommand>
{
    public AddSectionCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Course ID is invalid.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Order index must be zero or greater.");
    }
}