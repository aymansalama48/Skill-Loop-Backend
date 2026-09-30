using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;

public sealed class UpdateSessionCommandValidator : AbstractValidator<UpdateSessionCommand>
{
    public UpdateSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("This field is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid value.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("This field is required.")
            .MaximumLength(SessionDefaults.MaxTitleLength).WithMessage("Length exceeds the maximum allowed.");

        RuleFor(x => x.Description)
            .MaximumLength(SessionDefaults.MaxDescriptionLength)
            .WithMessage("Length exceeds the maximum allowed.");

        RuleFor(x => x.ScheduledAtUtc)
            .Must(d => !d.HasValue || d.Value > DateTime.UtcNow)
            .WithMessage("Invalid value.");

        RuleFor(x => x.DurationMinutes)
            .Must(d => !d.HasValue || (d >= 15 && d <= SessionDefaults.MaxDurationMinutes))
            .WithMessage($"مدة الجلسة لازم تكون بين 15 و {SessionDefaults.MaxDurationMinutes} دقيقة.");

        RuleFor(x => x.CreditsPrice)
            .Must(c => !c.HasValue || c >= 0)
            .WithMessage("Invalid value.");

        RuleFor(x => x.LocationDetails)
            .MaximumLength(SessionDefaults.MaxLocationDetailsLength)
            .WithMessage("Invalid value.");

        RuleFor(x => x.MaxParticipants)
            .Must(m => !m.HasValue || (m >= 1 && m <= SessionDefaults.MaxParticipants))
            .WithMessage($"عدد المشاركين يجب أن يكون بين 1 و {SessionDefaults.MaxParticipants}.");

        RuleFor(x => x.LocationType)
            .Must(l => !l.HasValue || Enum.IsDefined(l.Value))
            .WithMessage("Invalid value.");
    }
}
