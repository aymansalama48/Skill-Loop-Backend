using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        // 1. بناء الاستعلام مع AsNoTracking لتحسين الأداء
        var categoriesQuery = _context.AsNoTracking(_context.Categories)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.IconUrl,
                c.Description,
                c.DisplayOrder,
                c.Courses.Count(crs => crs.Status == CourseStatus.Published)));

        // 2. التنفيذ الفعلي على الداتا بيز (الكاش هيتدخل هنا تلقائياً عن طريق الـ CachingBehavior)
        var categories = await _context.ToListAsync(categoriesQuery, cancellationToken);

        return Result<IReadOnlyList<CategoryResponse>>.Success((IReadOnlyList<CategoryResponse>)categories);
    }
}