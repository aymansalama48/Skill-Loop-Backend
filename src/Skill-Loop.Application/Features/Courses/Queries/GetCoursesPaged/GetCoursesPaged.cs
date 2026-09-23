using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Courses.DTOs;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCoursesPaged;

public enum CourseSortOption
{
    Newest = 0,
    Popularity = 1,
    HighestRated = 2,
    PriceLowToHigh = 3,
    PriceHighToLow = 4
}

public sealed class GetCoursesPagedQuery : PaginationParameters, IQuery<PagedResult<CourseSummaryDto>>
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public CourseLevel? Level { get; init; }
    public int? MaxCredits { get; init; }
    public double? MinRating { get; init; }
    public CourseSortOption SortBy { get; init; } = CourseSortOption.Newest;

    public string GenerateCacheKey() =>
        $"courses:paged:q={SearchTerm?.Trim().ToLowerInvariant()}:cat={CategoryId}:lvl={Level}:maxP={MaxCredits}:minR={MinRating}:sort={SortBy}:p={PageNumber}:s={PageSize}";
}

public sealed class GetCoursesPagedQueryValidator : AbstractValidator<GetCoursesPagedQuery>
{
    public GetCoursesPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.MinRating).InclusiveBetween(0, 5).When(x => x.MinRating.HasValue);
        RuleFor(x => x.MaxCredits).GreaterThanOrEqualTo(0).When(x => x.MaxCredits.HasValue);
    }
}

public sealed class GetCoursesPagedQueryHandler : IQueryHandler<GetCoursesPagedQuery, PagedResult<CourseSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetCoursesPagedQueryHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<PagedResult<CourseSummaryDto>>> Handle(GetCoursesPagedQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = request.GenerateCacheKey();

        var cachedResult = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var query = _context.AsNoTracking(_context.Courses)
                    .Include(c => c.Category)
                    .Where(c => c.Status == CourseStatus.Published && !c.IsDeleted);

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
                    query = query.Where(c => c.Price.Credits <= request.MaxCredits.Value);
                }

                if (request.MinRating.HasValue)
                {
                    query = query.Where(c => c.Rating.AverageRating >= request.MinRating.Value);
                }

                query = request.SortBy switch
                {
                    CourseSortOption.Popularity => query.OrderByDescending(c => c.Rating.TotalReviews),
                    CourseSortOption.HighestRated => query.OrderByDescending(c => c.Rating.AverageRating),
                    CourseSortOption.PriceLowToHigh => query.OrderBy(c => c.Price.Credits),
                    CourseSortOption.PriceHighToLow => query.OrderByDescending(c => c.Price.Credits),
                    _ => query.OrderByDescending(c => c.CreatedAt)
                };

                var totalCount = await _context.CountAsync(query, ct);

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
                             c.Level.ToString(),
                             c.Status.ToString(),
                             c.Price.Credits,
                             c.Price.IsFree,
                             c.TotalLessonsCount,
                             c.TotalDuration.TotalMinutes,
                             c.Rating.AverageRating,
                             c.Rating.TotalReviews,
                             c.CreatedAt)),
                    ct);

                return new PagedResult<CourseSummaryDto>
                {
                    Items = items,
                    Pagination = new PaginationMetadata
                    {
                        TotalCount = totalCount,
                        CurrentPage = request.PageNumber,
                        PageSize = request.PageSize
                    }
                };
            },
            shouldCache: res => res != null && res.Items.Count > 0,
            slidingExpiration: TimeSpan.FromMinutes(10),
            absoluteExpiration: TimeSpan.FromHours(1),
            cancellationToken: cancellationToken);

        return Result<PagedResult<CourseSummaryDto>>.Success(cachedResult!);
    }
}
