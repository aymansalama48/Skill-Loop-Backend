using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Commands.AddInstructorAvailability;

public sealed class AddInstructorAvailabilityCommandValidator : AbstractValidator<AddInstructorAvailabilityCommand>
{
    public AddInstructorAvailabilityCommandValidator()
    {
        RuleFor(x => x.InstructorProfileId).NotEmpty().WithMessage("معرّف المدرب مطلوب.");
        RuleFor(x => x.DayOfWeek).IsInEnum().WithMessage("اليوم غير صالح.");
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("وقت البداية يجب أن يكون قبل وقت النهاية.");
    }
}