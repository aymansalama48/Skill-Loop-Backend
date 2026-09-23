using Skill_Loop.Application.Common.Abstractions.External.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.SiteSettings.Queries.GetSiteSettings
{
    public sealed record GetSiteSettingsQuery
        : ICacheableQuery<SiteSettingsRespone>
    {
        public string CacheKey => "site-settings";

        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);

        public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(2);
    }
}
