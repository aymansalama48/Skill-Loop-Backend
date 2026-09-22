using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Courses.DTOs;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCategories;

public sealed record GetCategoriesQuery : IQuery<IReadOnlyList<CategoryDto>>;

public sealed class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetCategoriesQueryHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = "categories:all";

        var list = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var categories = await _context.ToListAsync(
                    _context.AsNoTracking(_context.Categories)
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new CategoryDto(
                            c.Id,
                            c.Name,
                            c.Slug,
                            c.IconUrl,
                            c.Description,
                            c.DisplayOrder,
                            c.Courses.Count(crs => crs.Status == Domain.Enums.CourseStatus.Published))),
                    ct);

                return (IReadOnlyList<CategoryDto>)categories;
            },
            shouldCache: res => res != null && res.Count > 0,
            slidingExpiration: TimeSpan.FromHours(1),
            absoluteExpiration: TimeSpan.FromDays(1),
            cancellationToken: cancellationToken);

        return Result<IReadOnlyList<CategoryDto>>.Success(list ?? []);
    }
}
