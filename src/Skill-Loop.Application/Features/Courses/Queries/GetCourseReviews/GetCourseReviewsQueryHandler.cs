using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Courses.DTOs;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCourseReviews;

public sealed class GetCourseReviewsQueryHandler : IQueryHandler<GetCourseReviewsQuery, PagedResult<CourseReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCourseReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<CourseReviewDto>>> Handle(GetCourseReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Courses
            .Where(c => c.Id == request.CourseId)
            .SelectMany(c => c.Reviews)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new CourseReviewDto(
                r.Id,
                r.CourseId,
                r.UserId,
                r.Stars,
                r.Comment,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        var pagination = new PaginationMetadata { TotalCount = totalCount, PageSize = request.PageSize, CurrentPage = request.PageNumber };
        var pagedList = new PagedResult<CourseReviewDto> { Items = items, Pagination = pagination };

        return Result<PagedResult<CourseReviewDto>>.Success(pagedList);
    }
}
