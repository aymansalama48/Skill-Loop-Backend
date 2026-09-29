using FluentValidation;

namespace Skill_Loop.Application.Features.Bookings.Commands.CompleteBooking;

public sealed class CompleteBookingCommandValidator : AbstractValidator<CompleteBookingCommand>
{
    public CompleteBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("معرف الحجز مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف الحجز غير صالح.");
    }
}
