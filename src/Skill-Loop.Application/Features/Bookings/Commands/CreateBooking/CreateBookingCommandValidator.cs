using FluentValidation;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("معرف الجلسة مطلوب.");

        RuleFor(x => x.ScheduleDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("لا يمكن الحجز في تاريخ ماضي.");

        RuleFor(x => x.StartTime).NotEmpty().WithMessage("وقت الجلسة مطلوب.");
    }
}