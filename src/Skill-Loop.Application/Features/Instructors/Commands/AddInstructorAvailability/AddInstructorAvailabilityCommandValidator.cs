using FluentValidation;

namespace Skill_Loop.Application.Features.Instructors.Commands.AddInstructorAvailability;

public sealed class AddInstructorAvailabilityCommandValidator : AbstractValidator<AddInstructorAvailabilityCommand>
{
    public AddInstructorAvailabilityCommandValidator()
    {
        RuleFor(x => x.InstructorProfileId).NotEmpty().WithMessage("This field is required.");
        RuleFor(x => x.DayOfWeek).IsInEnum().WithMessage("Invalid value.");
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("Invalid value.");
    }
}