using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Courses.DTOs;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCourseById;

public sealed record GetCourseByIdQuery(Guid CourseId) : IQuery<CourseDetailDto>;

public sealed class GetCourseByIdQueryHandler : IQueryHandler<GetCourseByIdQuery, CourseDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetCourseByIdQueryHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result<CourseDetailDto>> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"courses:detail:{request.CourseId}";

        var detail = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var query = _context.AsNoTracking(_context.Courses)
                    .Include(c => c.Category)
                    .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                    .Where(c => c.Id == request.CourseId && !c.IsDeleted);

                var course = await _context.FirstOrDefaultAsync(query, ct);
                if (course is null)
                {
                    return null;
                }

                var sections = course.Sections.Select(s => new SectionDto(
                    s.Id,
                    s.Title,
                    s.OrderIndex,
                    s.Lessons.Select(l => new LessonDto(
                        l.Id,
                        l.Title,
                        l.OrderIndex,
                        l.IsPreviewable,
                        l.Video.Duration.TotalMinutes,
                        l.Video.StreamingResolution,
                        l.IsPreviewable ? l.Video.VideoUrl : null,
                        l.Resources.Select(r => new AttachmentDto(r.FileName, r.StorageUrl, r.FileSizeBytes)).ToList()
                    )).ToList()
                )).ToList();

                var attachments = course.Attachments.Select(a =>
                    new AttachmentDto(a.FileName, a.StorageUrl, a.FileSizeBytes)).ToList();

                return new CourseDetailDto(
                    course.Id,
                    course.Title,
                    course.Description,
                    course.ThumbnailUrl,
                    course.CategoryId,
                    course.Category != null ? course.Category.Name : string.Empty,
                    course.InstructorId,
                    course.InstructorName,
                    course.Level.ToString(),
                    course.Status.ToString(),
                    course.Price.Credits,
                    course.Price.IsFree,
                    course.TotalLessonsCount,
                    course.TotalDuration.TotalMinutes,
                    course.Rating.AverageRating,
                    course.Rating.TotalReviews,
                    sections,
                    attachments);
            },
            shouldCache: res => res != null,
            slidingExpiration: TimeSpan.FromMinutes(30),
            absoluteExpiration: TimeSpan.FromHours(4),
            cancellationToken: cancellationToken);

        if (detail is null)
        {
            return Result<CourseDetailDto>.Failure(new Error("Course.NotFound", "Course was not found.", ErrorType.NotFound));
        }

        return Result<CourseDetailDto>.Success(detail);
    }
}
