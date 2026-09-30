using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Categories.Queries.GetCategories;

[AllowAnonymous]
public sealed record GetCategoriesQuery : ICacheableQuery<IReadOnlyList<CategoryResponse>>
{
    public string CacheKey => "categories:all";
    public TimeSpan? SlidingExpiration => TimeSpan.FromHours(1);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromDays(1);
}