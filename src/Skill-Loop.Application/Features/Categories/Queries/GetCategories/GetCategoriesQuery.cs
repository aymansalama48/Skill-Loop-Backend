using Skill_Loop.Application.Common.Abstractions.External.Cache;

namespace Skill_Loop.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : ICacheableQuery<IReadOnlyList<CategoryResponse>>
{
    public string CacheKey => "categories:all";
    public TimeSpan? SlidingExpiration => TimeSpan.FromHours(1);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromDays(1);
}