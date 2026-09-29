using FluentValidation;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("معرف الجلسة مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف الجلسة غير صالح.");

        RuleFor(x => x.LearnerUserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف المستخدم غير صالح.");
    }
}
