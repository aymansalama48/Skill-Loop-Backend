using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Commands.UpdateCourseDetails;

public sealed class UpdateCourseDetailsCommandValidator : AbstractValidator<UpdateCourseDetailsCommand>
{
    public UpdateCourseDetailsCommandValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(c => c.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required.");

        RuleFor(c => c.Credits)
            .GreaterThanOrEqualTo(0).WithMessage("Credits cannot be negative.");
    }
}
