using Skill_Loop.Application.Features.Courses.Queries.GetCoursesPaged;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Api.Contracts.Courses;

public sealed record CreateCourseRequest(
    string Title,
    string Description,
    int Credits,
    CourseLevel Level,
    Guid CategoryId);
    
public sealed record AddLessonRequest(
    string Title,
    int OrderIndex,
    bool IsPreviewable);

public sealed record AddCourseReviewRequest(
    int Stars,
    string? Comment);

public sealed record UpdateCourseReviewRequest(
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


public sealed record AddSectionRequest(string Title, int OrderIndex);

public sealed record UpdateCourseDetailsRequest(
    string Title,
    string Description,
    int Credits,
    CourseLevel Level,
    Guid CategoryId);

public sealed record UpdateSectionRequest(string Title, int OrderIndex);

public sealed record ReorderSectionsRequest(Dictionary<Guid, int> SectionOrders);

public sealed record UpdateLessonRequest(
    string Title,
    int OrderIndex,
    bool IsPreviewable);

public sealed record ReorderLessonsRequest(Dictionary<Guid, int> LessonOrders);

public sealed record UploadCourseMaterialRequest(Microsoft.AspNetCore.Http.IFormFile File, Skill_Loop.Domain.Enums.MaterialType? MaterialType);

public sealed record UploadLessonMaterialRequest(Microsoft.AspNetCore.Http.IFormFile File, Skill_Loop.Domain.Enums.MaterialType? MaterialType);
