using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Courses.Commands.UpdateCourseReview;

/// <summary>
/// أمر تحديث تقييم دورة موجودة
/// </summary>
public sealed record UpdateCourseReviewCommand(
    Guid CourseId,
    Guid UserId,
    int Stars,
    string? Comment) : ICommand;

public sealed class UpdateCourseReviewCommandValidator : AbstractValidator<UpdateCourseReviewCommand>
{
    public UpdateCourseReviewCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Stars).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
