using FluentValidation;

namespace Skill_Loop.Application.Features.Bookings.Commands.CancelBooking;

public sealed class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("معرف الحجز مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف الحجز غير صالح.");

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف المستخدم غير صالح.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("سبب الإلغاء لا يجب أن يتجاوز 500 حرف.");
    }
}
