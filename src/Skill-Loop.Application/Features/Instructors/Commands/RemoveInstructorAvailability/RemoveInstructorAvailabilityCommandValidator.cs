using FluentValidation;
using System;

namespace Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorAvailability
{
    public class RemoveInstructorAvailabilityCommandValidator : AbstractValidator<RemoveInstructorAvailabilityCommand>
    {
        public RemoveInstructorAvailabilityCommandValidator()
        {
            RuleFor(x => x.InstructorProfileId).NotEmpty().WithMessage("معرّف المدرب مطلوب.");
            RuleFor(x => x.AvailabilityId).NotEmpty().WithMessage("معرّف التوفر مطلوب.");

        }
    }
}
