using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization.Models;

namespace Skill_Loop.Application.Features.Accounts.PermissionManagement.Queries.GetAllPermissions;

[AuthenticatedOnly]
public sealed record GetAllPermissionsQuery() : ICacheableQuery<List<PermissionDto>> 
{
    public string CacheKey => "Permissions:All"; 

    // 👇 التعديل: إطالة مدة الكاش (الصلاحيات الأساسية للنظام مش بتتغير تقريباً)
    public TimeSpan? SlidingExpiration => TimeSpan.FromDays(1);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromDays(7);
}
