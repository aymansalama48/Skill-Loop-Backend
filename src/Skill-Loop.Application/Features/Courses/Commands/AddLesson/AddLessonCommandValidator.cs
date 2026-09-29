using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Commands.AddLesson;

public sealed class AddLessonCommandValidator : AbstractValidator<AddLessonCommand>
{
    public AddLessonCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Course ID is invalid.");

        RuleFor(x => x.SectionId)
            .NotEmpty().WithMessage("Section ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Section ID is invalid.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.VideoUrl)
            .NotEmpty().WithMessage("Video URL is required.");

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage("Duration must be greater than zero.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Order index must be zero or greater.");
    }
}