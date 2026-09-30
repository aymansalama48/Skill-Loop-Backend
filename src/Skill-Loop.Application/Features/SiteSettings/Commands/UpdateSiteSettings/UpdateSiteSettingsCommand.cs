using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.SiteSettings.Commands.UpdateSiteSettings;

/// <summary>
/// Overwrites the public site settings (support address, social links, logo).
///
/// Security: this was [AuthenticatedOnly], which meant any registered learner could repoint
/// SupportEmail at a domain they control. Because that same address is the sender identity
/// for password-reset mail, this is a phishing primitive, not just a cosmetic edit. It now
/// requires an explicit permission and is role-gated to SuperAdmin at the controller.
///
/// Merge note: origin/main replaced this with the coarse SiteSettings.Manage umbrella.
/// Kept as the granular Update - it is the tighter grant, and Manage still maps to Update
/// via the umbrella in Permissions, so an admin holding Manage is unaffected.
/// </summary>
[Permission(Permissions.SiteSettings.Update)]
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