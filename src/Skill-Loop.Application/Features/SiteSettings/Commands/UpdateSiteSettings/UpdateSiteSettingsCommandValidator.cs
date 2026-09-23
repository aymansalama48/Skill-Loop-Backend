using FluentValidation;

namespace Skill_Loop.Application.Features.SiteSettings.Commands.UpdateSiteSettings;

internal sealed class UpdateSiteSettingsCommandValidator : AbstractValidator<UpdateSiteSettingsCommand>
{
    public UpdateSiteSettingsCommandValidator()
    {
        RuleFor(x => x.SupportEmail)
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صالحة.")
            .When(x => !string.IsNullOrWhiteSpace(x.SupportEmail));

        RuleFor(x => x.WebsiteUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("رابط الموقع الإلكتروني غير صالح.")
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl));

        RuleFor(x => x.FacebookUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("رابط فيسبوك غير صالح.")
            .When(x => !string.IsNullOrWhiteSpace(x.FacebookUrl));

        RuleFor(x => x.InstagramUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("رابط إنستجرام غير صالح.")
            .When(x => !string.IsNullOrWhiteSpace(x.InstagramUrl));
    }
}