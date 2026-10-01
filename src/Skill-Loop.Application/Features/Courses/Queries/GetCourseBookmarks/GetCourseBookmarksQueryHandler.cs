using Skill_Loop.Application.Common.Errors.Auth;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
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
    private readonly IUserManagementService _userService;

    public GetCourseBookmarksQueryHandler(IApplicationDbContext context, ICurrentUser currentUser, IUserManagementService userService)
    {
        _context = context;
        _currentUser = currentUser;
        _userService = userService;
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
                null, // InstructorAvatarUrl
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

        var instructorIds = items.Select(c => c.InstructorId).Distinct().ToList();
        var instructors = await _userService.GetUsersByIdsAsync(instructorIds, cancellationToken);
        var instructorAvatars = instructors.ToDictionary(u => u.Id, u => u.AvatarUrl);

        var finalItems = items.Select(item => item with 
        { 
            InstructorAvatarUrl = instructorAvatars.GetValueOrDefault(item.InstructorId) 
        }).ToList();

        var pagination = new PaginationMetadata { TotalCount = totalCount, PageSize = request.PageSize, CurrentPage = request.PageNumber };
        var pagedList = new PagedResult<CourseSummaryDto> { Items = finalItems, Pagination = pagination };

        return Result<PagedResult<CourseSummaryDto>>.Success(pagedList);
    }
}
