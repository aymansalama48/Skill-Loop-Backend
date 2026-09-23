using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Abstractions.Settings;
using Skill_Loop.Domain.Entities.SiteSettings;

namespace Skill_Loop.Infrastructure.Settings;

internal sealed class SiteSettingsService : ISiteSettingsService
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "Internal_SiteSettings";

    public SiteSettingsService(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<SiteSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        // 1. قراءة الإعدادات من الذاكرة
        if (_cache.TryGetValue(CacheKey, out SiteSettings? settings) && settings is not null)
        {
            return settings;
        }

        // 2. إذا لم تكن في الذاكرة، نجلبها من قاعدة البيانات
        settings = await _context.SiteSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        // 3. إذا لم يقم الأدمن بإضافة إعدادات بعد، نرجع كائن فارغ
        settings ??= new SiteSettings();

        // 4. إعداد خيارات الكاش مع تحديد الحجم (Size) لحل المشكلة 🚀
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSize(1); // 👈 تم تحديد الحجم هنا ليتوافق مع SizeLimit

        // 5. حفظها في الذاكرة
        _cache.Set(CacheKey, settings, cacheOptions);

        return settings;
    }
}