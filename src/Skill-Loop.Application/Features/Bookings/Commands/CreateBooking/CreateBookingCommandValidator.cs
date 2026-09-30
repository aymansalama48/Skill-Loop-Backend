using FluentValidation;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");

        RuleFor(x => x.LearnerUserId)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");
    }
}
