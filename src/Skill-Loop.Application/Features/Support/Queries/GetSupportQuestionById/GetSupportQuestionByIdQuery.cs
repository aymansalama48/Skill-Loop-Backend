using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Features.Support.Share;

namespace Skill_Loop.Application.Features.Support.Queries.GetSupportQuestionById;

/// <summary>
/// سؤال واحد من الـ FAQ العام — المنشور بس، وبشكل مبسّط.
/// </summary>
public sealed record GetSupportQuestionByIdQuery(Guid Id) : ICacheableQuery<SupportQuestionResponse>
{
    public string CacheKey => $"support:question:{Id}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}
