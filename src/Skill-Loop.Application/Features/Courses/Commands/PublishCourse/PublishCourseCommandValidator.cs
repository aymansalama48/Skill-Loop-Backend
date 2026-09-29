using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Commands.PublishCourse;

public sealed class PublishCourseCommandValidator : AbstractValidator<PublishCourseCommand>
{
    public PublishCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Course ID is invalid.");
    }
}