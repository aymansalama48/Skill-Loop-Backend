using Skill_Loop.Application.Common.Errors.Auth;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Courses.DTOs;
using Skill_Loop.Domain.Common.Results;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCourseBookmarks;

public sealed class GetCourseBookmarksQueryHandler : IQueryHandler<GetCourseBookmarksQuery, PagedResult<CourseSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCourseBookmarksQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<CourseSummaryDto>>> Handle(GetCourseBookmarksQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        if (userId == Guid.Empty)
        {
            return Result<PagedResult<CourseSummaryDto>>.Failure(AuthErrors.Unauthorized);
        }

        var query = _context.CourseBookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.Course)
            .ThenInclude(c => c!.Category)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => b.Course!)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseSummaryDto(
                c.Id,
                c.Title,
                c.Description,
                c.ThumbnailUrl,
                c.CategoryId,
                c.Category != null ? c.Category.Name : string.Empty,
                c.InstructorId,
                c.InstructorName,
                c.Level.ToString(),
                c.Status.ToString(),
                c.Credits,
                c.Credits == 0,
                c.TotalLessonsCount,
                c.TotalDuration.TotalMinutes,
                c.AverageRating,
                c.TotalReviews,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        var pagination = new PaginationMetadata { TotalCount = totalCount, PageSize = request.PageSize, CurrentPage = request.PageNumber };
        var pagedList = new PagedResult<CourseSummaryDto> { Items = items, Pagination = pagination };

        return Result<PagedResult<CourseSummaryDto>>.Success(pagedList);
    }
}
