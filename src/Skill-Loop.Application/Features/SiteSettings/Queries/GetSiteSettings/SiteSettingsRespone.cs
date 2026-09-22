namespace Skill_Loop.Application.Features.SiteSettings.Queries.GetSiteSettings;

public sealed class SiteSettingsRespone
{
    public string AppName { get; init; } = string.Empty;

    public string SupportEmail { get; init; } = string.Empty;

    public string ContactPhoneNumber { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string WebsiteUrl { get; init; } = string.Empty;

    public string FacebookUrl { get; init; } = string.Empty;

    public string InstagramUrl { get; init; } = string.Empty;

    public string WhatsAppNumber { get; init; } = string.Empty;
}