using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Commands.ReorderLessons;

public sealed class ReorderLessonsCommandValidator : AbstractValidator<ReorderLessonsCommand>
{
    public ReorderLessonsCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.LessonOrders).NotEmpty();
    }
}
