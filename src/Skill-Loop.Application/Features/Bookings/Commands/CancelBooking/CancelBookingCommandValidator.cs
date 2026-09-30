using FluentValidation;

namespace Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;

public sealed class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Length exceeds the maximum allowed.");
    }
}
