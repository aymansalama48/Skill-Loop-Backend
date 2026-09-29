using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Commands.ToggleCourseBookmark;

public sealed class ToggleCourseBookmarkCommandValidator : AbstractValidator<ToggleCourseBookmarkCommand>
{
    public ToggleCourseBookmarkCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID is invalid.");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Course ID is invalid.");
    }
}