using Skill_Loop.Domain.Entities.SiteSettings;

namespace Skill_Loop.Application.Common.Abstractions.Settings;

public interface ISiteSettingsService
{
    // نرجع الموديل الأساسي للإعدادات، أو الـ Entity مباشرة لو فضلت
    Task<SiteSettings> GetSettingsAsync(CancellationToken cancellationToken = default);
}