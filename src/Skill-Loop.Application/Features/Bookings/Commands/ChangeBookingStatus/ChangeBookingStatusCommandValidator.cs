using FluentValidation;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Bookings.Commands.ChangeBookingStatus;

public sealed class ChangeBookingStatusCommandValidator : AbstractValidator<ChangeBookingStatusCommand>
{
    // المحاضر هو اللي بيغيّر الحالة — مينفعش يحوّل الحجز لـ Cancelled (ده للطرفين)
    private static readonly BookingStatus[] InstructorAllowedStatuses =
    [
        BookingStatus.Confirmed,
        BookingStatus.InProgress,
        BookingStatus.Completed,
        BookingStatus.Rejected,
        BookingStatus.NoShow
    ];

    public ChangeBookingStatusCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");

        RuleFor(x => x.InstructorUserId)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");

        RuleFor(x => x.NewStatus)
            .Must(s => InstructorAllowedStatuses.Contains(s))
            .WithMessage("This field is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Length exceeds the maximum allowed.");

        RuleFor(x => x)
            .Must(x => x.NewStatus != BookingStatus.Rejected || !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage("This field is required.");
    }
}
