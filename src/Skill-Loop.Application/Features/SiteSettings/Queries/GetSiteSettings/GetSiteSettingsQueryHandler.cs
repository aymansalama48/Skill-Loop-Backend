using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;
using Microsoft.EntityFrameworkCore; // لو AsNoTracking و FirstOrDefaultAsync من EF Core

namespace Skill_Loop.Application.Features.SiteSettings.Queries.GetSiteSettings;

internal sealed class GetSiteSettingsQueryHandler
    : IQueryHandler<GetSiteSettingsQuery, SiteSettingsRespone>
{
    private readonly IApplicationDbContext _context;

    public GetSiteSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SiteSettingsRespone>> Handle(
        GetSiteSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _context.FirstOrDefaultAsync(
            _context.AsNoTracking(_context.SiteSettings),
            cancellationToken);

        if (settings is null)
        {
            // ✅ الصح: اسم الكلاس Result<T> نفسه، مش Result.Failure<T>
            return Result<SiteSettingsRespone>.Failure(
                "لم يتم العثور على إعدادات الموقع.");
        }

        var response = new SiteSettingsRespone
        {
            AppName = settings.AppName,
            SupportEmail = settings.SupportEmail,
            ContactPhoneNumber = settings.ContactPhoneNumber,
            Address = settings.Address,
            WebsiteUrl = settings.WebsiteUrl,
            FacebookUrl = settings.FacebookUrl,
            InstagramUrl = settings.InstagramUrl,
            WhatsAppNumber = settings.WhatsAppNumber
        };

        return Result<SiteSettingsRespone>.Success(response);
    }
}