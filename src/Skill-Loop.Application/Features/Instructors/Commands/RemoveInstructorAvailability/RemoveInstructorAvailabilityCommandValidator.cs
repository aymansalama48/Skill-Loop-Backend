using FluentValidation;
using System;

namespace Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorAvailability
{
    public class RemoveInstructorAvailabilityCommandValidator : AbstractValidator<RemoveInstructorAvailabilityCommand>
    {
        public RemoveInstructorAvailabilityCommandValidator()
        {
            RuleFor(x => x.InstructorProfileId).NotEmpty().WithMessage("This field is required.");
            RuleFor(x => x.AvailabilityId).NotEmpty().WithMessage("This field is required.");

        }
    }
}
