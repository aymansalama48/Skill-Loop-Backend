using FluentValidation;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.Courses.Commands.DeleteCourseReview;

/// <summary>
/// أمر حذف تقييم دورة
/// </summary>
public sealed record DeleteCourseReviewCommand(Guid CourseId, Guid UserId) : ICommand;

public sealed class DeleteCourseReviewCommandValidator : AbstractValidator<DeleteCourseReviewCommand>
{
    public DeleteCourseReviewCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
