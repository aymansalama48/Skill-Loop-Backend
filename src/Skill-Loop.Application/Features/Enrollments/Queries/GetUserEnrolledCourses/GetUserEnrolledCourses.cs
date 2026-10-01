using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Enrollments.DTOs;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Enrollments.Queries.GetUserEnrolledCourses;

[AuthenticatedOnly]
public sealed record GetUserEnrolledCoursesQuery(Guid UserId) : IQuery<IReadOnlyList<UserEnrolledCourseDto>>;

public sealed class GetUserEnrolledCoursesQueryHandler : IQueryHandler<GetUserEnrolledCoursesQuery, IReadOnlyList<UserEnrolledCourseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserManagementService _userService;

    public GetUserEnrolledCoursesQueryHandler(IApplicationDbContext context, IUserManagementService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<Result<IReadOnlyList<UserEnrolledCourseDto>>> Handle(GetUserEnrolledCoursesQuery request, CancellationToken cancellationToken)
    {
        var enrollmentsQuery = _context.AsNoTracking(_context.Enrollments)
            .Where(e => e.UserId == request.UserId)
            .OrderByDescending(e => e.EnrolledAt);

        var coursesDict = await _context.ToListAsync(
            _context.AsNoTracking(_context.Courses),
            cancellationToken);

        var enrollments = await _context.ToListAsync(enrollmentsQuery, cancellationToken);

        var instructorIds = coursesDict.Select(c => c.InstructorId).Distinct().ToList();
        var instructors = await _userService.GetUsersByIdsAsync(instructorIds, cancellationToken);
        var instructorAvatars = instructors.ToDictionary(u => u.Id, u => u.AvatarUrl);

        var result = enrollments.Select(e =>
        {
            var course = coursesDict.FirstOrDefault(c => c.Id == e.CourseId);
            return new UserEnrolledCourseDto(
                e.Id,
                e.CourseId,
                course?.Title ?? "Unknown Course",
                course?.ThumbnailUrl ?? string.Empty,
                course?.InstructorName ?? string.Empty,
                course != null ? instructorAvatars.GetValueOrDefault(course.InstructorId) : null,
                e.ProgressPercentage,
                e.Status.ToString(),
                e.LastWatchedLessonId,
                course?.TotalLessonsCount ?? 0,
                e.EnrolledAt);
        }).ToList();

        return Result<IReadOnlyList<UserEnrolledCourseDto>>.Success(result);
    }
}
