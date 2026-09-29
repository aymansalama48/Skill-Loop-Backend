using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Features.Courses.DTOs;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Courses.Queries.GetCourseById;

public sealed class GetCourseByIdQueryHandler : IQueryHandler<GetCourseByIdQuery, CourseDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCourseByIdQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<CourseDetailDto>> Handle(
        GetCourseByIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.AsNoTracking(_context.Courses)
            .Include(c => c.Category)
            .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
            .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
            .Where(c => c.Id == request.CourseId && !c.IsDeleted);

        var course = await _context.FirstOrDefaultAsync(query, cancellationToken);
        if (course is null)
        {
            return Result<CourseDetailDto>.Failure(
                new Error("Course.NotFound", "Course was not found.", ErrorType.NotFound));
        }

        bool isAdmin = _currentUser.IsInRole("Admin") || _currentUser.IsInRole("SuperAdmin");
        bool isInstructor = _currentUser.UserId == course.InstructorId;
        bool isEnrolled = false;

        if (_currentUser.IsAuthenticated && !isAdmin && !isInstructor)
        {
            isEnrolled = await _context.AnyAsync(_context.Enrollments.Where(e => 
                e.CourseId == course.Id && 
                e.UserId == _currentUser.UserId), cancellationToken);
        }

        bool canAccessPremiumContent = isAdmin || isInstructor || isEnrolled;

        var sections = course.Sections.Select(s => new SectionDto(
            s.Id,
            s.Title,
            s.OrderIndex,
            s.Lessons.Select(l => new LessonDto(
                l.Id,
                l.Title,
                l.OrderIndex,
                l.IsPreviewable,
                l.Duration.TotalMinutes,
                l.StreamingResolution,
                (l.IsPreviewable || canAccessPremiumContent) ? l.VideoUrl : null,
                l.Resources.Select(r => new AttachmentDto(r.FileName, r.DriveFileId, r.SizeBytes)).ToList()
            )).ToList()
        )).ToList();

        var attachments = course.Attachments.Select(a =>
            new AttachmentDto(a.FileName, a.DriveFileId, a.SizeBytes)).ToList();

        var dto = new CourseDetailDto(
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
            course.Credits,
            course.IsFree,
            course.TotalLessonsCount,
            course.TotalDuration.TotalMinutes,
            course.AverageRating,
            course.TotalReviews,
            sections,
            attachments);

        return Result<CourseDetailDto>.Success(dto);
    }
}