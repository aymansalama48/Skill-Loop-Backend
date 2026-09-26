using FluentValidation;
using Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorReview;
using System;
using System.Collections.Generic;
using System.Text;

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
