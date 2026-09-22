namespace Skill_Loop.Application.Features.Courses.DTOs;

public sealed record CourseSummaryDto(
    Guid Id,
    string Title,
    string Description,
    string ThumbnailUrl,
    Guid CategoryId,
    string CategoryName,
    Guid InstructorId,
    string InstructorName,
    string Level,
    string Status,
    int Credits,
    bool IsFree,
    int TotalLessonsCount,
    double TotalDurationMinutes,
    double AverageRating,
    int TotalReviews,
    DateTime CreatedAt);

public sealed record CourseDetailDto(
    Guid Id,
    string Title,
    string Description,
    string ThumbnailUrl,
    Guid CategoryId,
    string CategoryName,
    Guid InstructorId,
    string InstructorName,
    string Level,
    string Status,
    int Credits,
    bool IsFree,
    int TotalLessonsCount,
    double TotalDurationMinutes,
    double AverageRating,
    int TotalReviews,
    IReadOnlyList<SectionDto> Sections,
    IReadOnlyList<AttachmentDto> Attachments);

public sealed record SectionDto(
    Guid Id,
    string Title,
    int OrderIndex,
    IReadOnlyList<LessonDto> Lessons);

public sealed record LessonDto(
    Guid Id,
    string Title,
    int OrderIndex,
    bool IsPreviewable,
    double DurationMinutes,
    string? StreamingResolution,
    string? VideoUrl,
    IReadOnlyList<AttachmentDto> Resources);

public sealed record AttachmentDto(
    string FileName,
    string StorageUrl,
    long FileSizeBytes);

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? IconUrl,
    string? Description,
    int DisplayOrder,
    int CourseCount);
