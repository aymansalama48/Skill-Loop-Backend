using FluentValidation;

namespace Skill_Loop.Application.Features.Courses.Commands.ReorderSections;

public sealed class ReorderSectionsCommandValidator : AbstractValidator<ReorderSectionsCommand>
{
    public ReorderSectionsCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.SectionOrders).NotEmpty();
    }
}
