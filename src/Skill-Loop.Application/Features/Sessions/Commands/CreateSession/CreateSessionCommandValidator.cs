using FluentValidation;

namespace Skill_Loop.Application.Features.Sessions.Commands.CreateSession;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان الجلسة مطلوب.")
            .MaximumLength(SessionDefaults.MaxTitleLength).WithMessage("عنوان الجلسة لا يجب أن يتجاوز 200 حرف.");

        RuleFor(x => x.InstructorId)
            .NotEmpty().WithMessage("معرف المحاضر مطلوب.")
            .NotEqual(Guid.Empty).WithMessage("معرف المحاضر غير صالح.");

        RuleFor(x => x.Description)
            .MaximumLength(SessionDefaults.MaxDescriptionLength)
            .WithMessage("وصف الجلسة لا يجب أن يتجاوز 2000 حرف.");

        RuleFor(x => x.ScheduledAtUtc)
            .Must(d => !d.HasValue || d.Value > DateTime.UtcNow)
            .WithMessage("موعد الجلسة لازم يكون في المستقبل.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, SessionDefaults.MaxDurationMinutes)
            .WithMessage($"مدة الجلسة لازم تكون بين 15 و {SessionDefaults.MaxDurationMinutes} دقيقة.");

        RuleFor(x => x.CreditsPrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر الجلسة لا يمكن أن يكون سالب.");

        RuleFor(x => x.LocationDetails)
            .MaximumLength(SessionDefaults.MaxLocationDetailsLength)
            .WithMessage("تفاصيل المكان لا يجب أن تتجاوز 500 حرف.");

        RuleFor(x => x.MaxParticipants)
            .InclusiveBetween(1, SessionDefaults.MaxParticipants)
            .WithMessage($"عدد المشاركين يجب أن يكون بين 1 و {SessionDefaults.MaxParticipants}.");

        RuleFor(x => x.LocationType)
            .IsInEnum().WithMessage("نوع المكان غير صالح.");
    }
}
