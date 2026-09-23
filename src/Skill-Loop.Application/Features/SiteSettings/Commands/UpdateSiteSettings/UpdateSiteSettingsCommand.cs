using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;

namespace Skill_Loop.Application.Features.SiteSettings.Commands.UpdateSiteSettings;

public sealed record UpdateSiteSettingsCommand(
    string? AppName,
    string? LogoName,
    string? SupportEmail,
    string? ContactPhoneNumber,
    string? Address,
    string? WebsiteUrl,
    string? FacebookUrl,
    string? InstagramUrl,
    string? WhatsAppNumber
) : ICommand<bool>, ICacheInvalidatorCommand
{
    // הـ Pipeline هيمسح كاش الـ Frontend وكاش الـ Internal Service أوتوماتيك
    public IReadOnlyCollection<string> CacheKeys => ["site-settings", "Internal_SiteSettings"];
}