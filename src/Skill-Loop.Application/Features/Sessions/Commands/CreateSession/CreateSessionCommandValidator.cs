using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.CreateSession;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("This field is required.")
            .MaximumLength(SessionDefaults.MaxTitleLength).WithMessage("Length exceeds the maximum allowed.");

        RuleFor(x => x.InstructorId)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");

        RuleFor(x => x.Description)
            .MaximumLength(SessionDefaults.MaxDescriptionLength)
            .WithMessage("Length exceeds the maximum allowed.");

        RuleFor(x => x.ScheduledAtUtc)
            .Must(d => !d.HasValue || d.Value > DateTime.UtcNow)
            .WithMessage("Invalid value.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, SessionDefaults.MaxDurationMinutes)
            .WithMessage($"مدة الجلسة لازم تكون بين 15 و {SessionDefaults.MaxDurationMinutes} دقيقة.");

        RuleFor(x => x.CreditsPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Invalid value.");

        RuleFor(x => x.LocationDetails)
            .MaximumLength(SessionDefaults.MaxLocationDetailsLength)
            .WithMessage("Invalid value.");

        RuleFor(x => x.MaxParticipants)
            .InclusiveBetween(1, SessionDefaults.MaxParticipants)
            .WithMessage($"عدد المشاركين يجب أن يكون بين 1 و {SessionDefaults.MaxParticipants}.");

        RuleFor(x => x.LocationType)
            .IsInEnum().WithMessage("Invalid value.");
    }
}
