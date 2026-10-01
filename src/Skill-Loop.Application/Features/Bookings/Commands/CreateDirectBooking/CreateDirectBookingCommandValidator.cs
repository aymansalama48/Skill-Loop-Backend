using FluentValidation;

namespace Skill_Loop.Application.Features.Bookings.Commands.CreateDirectBooking;

public sealed class CreateDirectBookingCommandValidator : AbstractValidator<CreateDirectBookingCommand>
{
    public CreateDirectBookingCommandValidator()
    {
        RuleFor(x => x.LearnerUserId).NotEmpty();
        RuleFor(x => x.InstructorId).NotEmpty();
        RuleFor(x => x.ScheduledAtUtc).NotEmpty().GreaterThan(System.DateTime.UtcNow);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
    }
}
