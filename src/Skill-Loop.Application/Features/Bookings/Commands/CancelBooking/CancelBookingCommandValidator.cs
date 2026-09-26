using FluentValidation;

namespace Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;

public sealed class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty().WithMessage("معرف الحجز مطلوب.");
    }
}