using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Courses.DTOs;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCoursesPaged;

public enum CourseSortOption
{
    Newest = 0,
    Popularity = 1,
    HighestRated = 2,
    PriceLowToHigh = 3,
    PriceHighToLow = 4
}

[AuthenticatedOnly]
public sealed class GetCoursesPagedQuery : PaginationParameters, ICacheableQuery<PagedResult<CourseSummaryDto>>
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public CourseLevel? Level { get; init; }
    public int? MaxCredits { get; init; }
    public double? MinRating { get; init; }
    public CourseSortOption SortBy { get; init; } = CourseSortOption.Newest;
    public CourseStatus? Status { get; init; }
    public Guid? InstructorId { get; init; }

    public string CacheKey =>
        $"courses:paged:q={SearchTerm?.Trim().ToLowerInvariant()}:cat={CategoryId}:lvl={Level}:maxP={MaxCredits}:minR={MinRating}:sort={SortBy}:status={Status}:inst={InstructorId}:p={PageNumber}:s={PageSize}";

    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromHours(1);
}