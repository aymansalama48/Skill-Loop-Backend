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
            .NotEmpty().WithMessage("معرف الحجز مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف الحجز غير صالح.");

        RuleFor(x => x.InstructorUserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف المستخدم غير صالح.");

        RuleFor(x => x.NewStatus)
            .Must(s => InstructorAllowedStatuses.Contains(s))
            .WithMessage("الحالة المطلوبة غير مسموح بها. الحالات المسموحة: Confirmed, InProgress, Completed, Rejected, NoShow.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("السبب لا يجب أن يتجاوز 500 حرف.");

        RuleFor(x => x)
            .Must(x => x.NewStatus != BookingStatus.Rejected || !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage("سبب الرفض مطلوب.");
    }
}
