using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Features.Sessions.Queries.Share;
using System;
using System.Collections.Generic;
using System.Text;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionById
{
    [AuthenticatedOnly]
public sealed record GetSessionByIdQuery(Guid Id) : ICacheableQuery<SessionResponse>
    {
        public string CacheKey => $"sessions:{Id}";
        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
        public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
    }
}
