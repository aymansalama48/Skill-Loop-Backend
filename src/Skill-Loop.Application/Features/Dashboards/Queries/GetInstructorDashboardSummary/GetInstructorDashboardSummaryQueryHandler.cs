using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Dashboards.Queries.GetInstructorDashboardSummary;

internal sealed class GetInstructorDashboardSummaryQueryHandler : IRequestHandler<GetInstructorDashboardSummaryQuery, Result<GetInstructorDashboardSummaryResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetInstructorDashboardSummaryQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<GetInstructorDashboardSummaryResponse>> Handle(GetInstructorDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        
        var userId = _currentUser.UserId;
        var instructor = await _context.FirstOrDefaultAsync(_context.InstructorProfiles, i => i.UserId == userId, cancellationToken);
        if (instructor == null) return Result<GetInstructorDashboardSummaryResponse>.Failure(new Error("Instructor.NotFound", "Instructor not found", ErrorType.NotFound));

        // Course.InstructorId and Session.InstructorId both hold ApplicationUser ids (UserId),
        // not the InstructorProfile primary key, so they must be matched on instructor.UserId.
        var courseIds = await _context.Courses.Where(c => c.InstructorId == instructor.UserId).Select(c => c.Id).ToListAsync(cancellationToken);
        var totalCourses = courseIds.Count;
        
        var totalEnrollments = 0;
        if (courseIds.Any()) {
            totalEnrollments = await _context.CountAsync(_context.Enrollments.Where(e => courseIds.Contains(e.CourseId)), cancellationToken);
        }

        var sessionIds = await _context.Sessions.Where(s => s.InstructorId == instructor.UserId).Select(s => s.Id).ToListAsync(cancellationToken);
        var totalSessions = sessionIds.Count;

        var totalBookings = 0;
        if (sessionIds.Any()) {
            totalBookings = await _context.CountAsync(_context.Bookings.Where(b => sessionIds.Contains(b.SessionId)), cancellationToken);
        }
        
        var wallet = await _context.FirstOrDefaultAsync(_context.UserWallets, w => w.UserId == userId, cancellationToken);
        var balance = wallet?.Balance ?? 0;
        

        var activeSessionsCount = await _context.Sessions
            .CountAsync(s => s.InstructorId == instructor.UserId && s.Status == Skill_Loop.Domain.Enums.SessionStatus.Published, cancellationToken);
            
        var completedSessionsCount = await _context.Sessions
            .CountAsync(s => s.InstructorId == instructor.UserId && s.Status == Skill_Loop.Domain.Enums.SessionStatus.Completed, cancellationToken);
            
        double averageRating = 0.0;
        if (courseIds.Any()) {
            var reviews = await _context.CourseReviews
                .Where(r => courseIds.Contains(r.CourseId))
                .Select(r => r.Stars)
                .ToListAsync(cancellationToken);
                
            if (reviews.Any()) {
                averageRating = reviews.Average();
            }
        }

        var response = new GetInstructorDashboardSummaryResponse(
            TotalCourses: totalCourses,
            TotalSessions: totalSessions,
            TotalBookings: totalBookings,
            TotalEnrollments: totalEnrollments,
            WalletBalance: balance,
            AverageRating: Math.Round(averageRating, 1),
            ActiveSessions: activeSessionsCount,
            CompletedSessions: completedSessionsCount
        );

        return Result<GetInstructorDashboardSummaryResponse>.Success(response);
    }
}
