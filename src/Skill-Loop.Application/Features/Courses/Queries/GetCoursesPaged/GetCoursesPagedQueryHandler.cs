using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Courses.DTOs;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCoursesPaged;

public sealed class GetCoursesPagedQueryHandler : IQueryHandler<GetCoursesPagedQuery, PagedResult<CourseSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManagementService _userService;

    public GetCoursesPagedQueryHandler(IApplicationDbContext context, IUserManagementService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<Result<PagedResult<CourseSummaryDto>>> Handle(
        GetCoursesPagedQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.AsNoTracking(_context.Courses)
            .Include(c => c.Category)
            .Where(c => !c.IsDeleted);

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        if (request.InstructorId.HasValue)
        {
            query = query.Where(c => c.InstructorId == request.InstructorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(c =>
                EF.Functions.Like(c.Title, term) ||
                EF.Functions.Like(c.Description, term) ||
                EF.Functions.Like(c.InstructorName, term));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == request.CategoryId.Value);
        }

        if (request.Level.HasValue)
        {
            query = query.Where(c => c.Level == request.Level.Value);
        }

        if (request.MaxCredits.HasValue)
        {
            query = query.Where(c => c.Credits <= request.MaxCredits.Value);
        }

        if (request.MinRating.HasValue)
        {
            query = query.Where(c => c.AverageRating >= request.MinRating.Value);
        }

        query = request.SortBy switch
        {
            CourseSortOption.Popularity => query.OrderByDescending(c => c.TotalReviews),
            CourseSortOption.HighestRated => query.OrderByDescending(c => c.AverageRating),
            CourseSortOption.PriceLowToHigh => query.OrderBy(c => c.Credits),
            CourseSortOption.PriceHighToLow => query.OrderByDescending(c => c.Credits),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var totalCount = await _context.CountAsync(query, cancellationToken);

        var items = await _context.ToListAsync(
            query.Skip(request.Skip)
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
                     c.IsFree,
                     c.TotalLessonsCount,
                     c.TotalDuration.TotalMinutes,
                     c.AverageRating,
                     c.TotalReviews,
                     c.CreatedAt)),
            cancellationToken);

        var instructorIds = items.Select(c => c.InstructorId).Distinct().ToList();
        var instructors = await _userService.GetUsersByIdsAsync(instructorIds, cancellationToken);
        var instructorAvatars = instructors.ToDictionary(u => u.Id, u => u.AvatarUrl);

        var finalItems = items.Select(item => item with 
        { 
            InstructorAvatarUrl = instructorAvatars.GetValueOrDefault(item.InstructorId) 
        }).ToList();

        var result = new PagedResult<CourseSummaryDto>
        {
            Items = finalItems,
            Pagination = new PaginationMetadata
            {
                TotalCount = totalCount,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize
            }
        };

        return Result<PagedResult<CourseSummaryDto>>.Success(result);
    }
}