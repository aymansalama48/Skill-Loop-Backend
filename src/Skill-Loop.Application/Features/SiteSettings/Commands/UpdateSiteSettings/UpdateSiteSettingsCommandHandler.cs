using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.SiteSettings.Commands.UpdateSiteSettings;

internal sealed class UpdateSiteSettingsCommandHandler : ICommandHandler<UpdateSiteSettingsCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateSiteSettingsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateSiteSettingsCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب الإعدادات الحالية
        var settings = await _context.SiteSettings.FirstOrDefaultAsync(cancellationToken);

        // 2. لو مفيش إعدادات، هننشئ واحدة جديدة
        if (settings is null)
        {
            settings = new Domain.Entities.SiteSettings.SiteSettings();
            _context.Add(settings);
        }

        // 3. تحديث القيم: لو القيمة المبعوتة null سيب القديمة زي ما هي
        settings.AppName = request.AppName ?? settings.AppName ?? string.Empty;
        settings.LogoName = request.LogoName ?? settings.LogoName ?? string.Empty;
        settings.SupportEmail = request.SupportEmail ?? settings.SupportEmail ?? string.Empty;
        settings.ContactPhoneNumber = request.ContactPhoneNumber ?? settings.ContactPhoneNumber ?? string.Empty;
        settings.Address = request.Address ?? settings.Address ?? string.Empty;
        settings.WebsiteUrl = request.WebsiteUrl ?? settings.WebsiteUrl ?? string.Empty;
        settings.FacebookUrl = request.FacebookUrl ?? settings.FacebookUrl ?? string.Empty;
        settings.InstagramUrl = request.InstagramUrl ?? settings.InstagramUrl ?? string.Empty;
        settings.WhatsAppNumber = request.WhatsAppNumber ?? settings.WhatsAppNumber ?? string.Empty;

        // 4. الحفظ في قاعدة البيانات
        await _context.SaveChangesAsync(cancellationToken);

        // 5. إرجاع النتيجة
        return Result<bool>.Success(true);
    }
}