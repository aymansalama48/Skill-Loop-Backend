using FluentValidation;

namespace Skill_Loop.Application.Features.SiteSettings.Commands.UpdateSiteSettings;

internal sealed class UpdateSiteSettingsCommandValidator : AbstractValidator<UpdateSiteSettingsCommand>
{
    public UpdateSiteSettingsCommandValidator()
    {
        RuleFor(x => x.AppName)
            .MaximumLength(200).WithMessage("Too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.AppName));

        RuleFor(x => x.LogoName)
            .MaximumLength(300).WithMessage("Too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.LogoName));

        RuleFor(x => x.SupportEmail)
            .EmailAddress().WithMessage("Invalid value.")
            .MaximumLength(320).WithMessage("Too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.SupportEmail));

        RuleFor(x => x.ContactPhoneNumber)
            .MaximumLength(50).WithMessage("Too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhoneNumber));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.WhatsAppNumber)
            .MaximumLength(50).WithMessage("Too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.WhatsAppNumber));

        // URLs are rendered by the front end as links, so only http/https are accepted.
        // Uri.TryCreate alone accepts javascript: and data:, which would let a settings
        // write become a stored XSS against every site visitor.
        RuleFor(x => x.WebsiteUrl)
            .Must(BeHttpUrl).WithMessage("Must be an absolute http(s) URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl));

        RuleFor(x => x.FacebookUrl)
            .Must(BeHttpUrl).WithMessage("Must be an absolute http(s) URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.FacebookUrl));

        RuleFor(x => x.InstagramUrl)
            .Must(BeHttpUrl).WithMessage("Must be an absolute http(s) URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.InstagramUrl));
    }

    private static bool BeHttpUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
            || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
    }
}
