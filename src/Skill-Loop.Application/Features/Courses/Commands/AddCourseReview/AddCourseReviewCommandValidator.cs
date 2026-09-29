using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Commands.AddCourseReview;

public sealed class AddCourseReviewCommandValidator : AbstractValidator<AddCourseReviewCommand>
{
    public AddCourseReviewCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.")
            .NotEqual(Guid.Empty).WithMessage("CourseId is invalid.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .NotEqual(Guid.Empty).WithMessage("UserId is invalid.");

        RuleFor(x => x.Stars)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5 stars.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("Comment cannot exceed 1000 characters.");
    }
}