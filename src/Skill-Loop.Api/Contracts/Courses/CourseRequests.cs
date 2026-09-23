using Skill_Loop.Application.Features.Courses.Queries.GetCoursesPaged;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Contracts.Courses;

public sealed record CreateCourseRequest(
    string Title,
    string Description,
    string ThumbnailUrl,
    int Credits,
    CourseLevel Level,
    Guid CategoryId);

public sealed record AddLessonRequest(
    string Title,
    string VideoUrl,
    TimeSpan Duration,
    int OrderIndex,
    bool IsPreviewable,
    string? StreamingResolution,
    string? ExternalProviderId);

public sealed record AddCourseReviewRequest(
    int Stars,
    string? Comment);

public sealed record GetCoursesRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    CourseLevel? Level = null,
    int? MaxCredits = null,
    double? MinRating = null,
    CourseSortOption SortBy = CourseSortOption.Newest);
