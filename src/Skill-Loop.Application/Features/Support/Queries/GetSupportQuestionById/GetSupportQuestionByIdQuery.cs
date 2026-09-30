using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionById;

/// <summary>
/// سؤال واحد من الـ FAQ العام — المنشور بس، وبشكل مبسّط.
/// </summary>
// Merge note: origin/main applied Support.View here, which would make the public FAQ
// question require a permission and break it for every logged-out visitor. The type's own
// summary states this record is the single question served without authentication.
[AllowAnonymous]
public sealed record GetSupportQuestionByIdQuery(Guid Id) : ICacheableQuery<SupportQuestionResponse>
{
    public string CacheKey => $"support:question:{Id}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}
