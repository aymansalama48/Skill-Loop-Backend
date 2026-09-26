using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.UpdateSession;

public sealed class UpdateSessionCommandValidator : AbstractValidator<UpdateSessionCommand>
{
    public UpdateSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف الجلسة مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف الجلسة غير صالح.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان الجلسة مطلوب.")
            .MaximumLength(SessionDefaults.MaxTitleLength).WithMessage("عنوان الجلسة لا يجب أن يتجاوز 200 حرف.");

        RuleFor(x => x.Description)
            .MaximumLength(SessionDefaults.MaxDescriptionLength)
            .WithMessage("وصف الجلسة لا يجب أن يتجاوز 2000 حرف.");

        RuleFor(x => x.ScheduledAtUtc)
            .Must(d => !d.HasValue || d.Value > DateTime.UtcNow)
            .WithMessage("موعد الجلسة لازم يكون في المستقبل.");

        RuleFor(x => x.DurationMinutes)
            .Must(d => !d.HasValue || (d >= 15 && d <= SessionDefaults.MaxDurationMinutes))
            .WithMessage($"مدة الجلسة لازم تكون بين 15 و {SessionDefaults.MaxDurationMinutes} دقيقة.");

        RuleFor(x => x.CreditsPrice)
            .Must(c => !c.HasValue || c >= 0)
            .WithMessage("سعر الجلسة لا يمكن أن يكون سالب.");

        RuleFor(x => x.LocationDetails)
            .MaximumLength(SessionDefaults.MaxLocationDetailsLength)
            .WithMessage("تفاصيل المكان لا يجب أن تتجاوز 500 حرف.");

        RuleFor(x => x.MaxParticipants)
            .Must(m => !m.HasValue || (m >= 1 && m <= SessionDefaults.MaxParticipants))
            .WithMessage($"عدد المشاركين يجب أن يكون بين 1 و {SessionDefaults.MaxParticipants}.");

        RuleFor(x => x.LocationType)
            .Must(l => !l.HasValue || Enum.IsDefined(l.Value))
            .WithMessage("نوع المكان غير صالح.");
    }
}
