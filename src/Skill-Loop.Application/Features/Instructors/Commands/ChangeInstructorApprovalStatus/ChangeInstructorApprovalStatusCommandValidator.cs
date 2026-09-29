using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Commands.ChangeInstructorApprovalStatus;

public sealed class ChangeInstructorApprovalStatusCommandValidator : AbstractValidator<ChangeInstructorApprovalStatusCommand>
{
    public ChangeInstructorApprovalStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرّف المستخدم مطلوب.");
    }
}