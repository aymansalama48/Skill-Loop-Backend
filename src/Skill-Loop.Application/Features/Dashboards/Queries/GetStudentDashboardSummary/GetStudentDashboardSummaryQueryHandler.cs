using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Dashboards.Queries.GetStudentDashboardSummary;

internal sealed class GetStudentDashboardSummaryQueryHandler : IRequestHandler<GetStudentDashboardSummaryQuery, Result<GetStudentDashboardSummaryResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetStudentDashboardSummaryQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<GetStudentDashboardSummaryResponse>> Handle(GetStudentDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        
        var userId = _currentUser.UserId;
        var totalCourses = 0;
        var totalSessions = 0;
        var totalBookings = await _context.CountAsync(_context.Bookings.Where(b => b.LearnerUserId == userId), cancellationToken);
        var totalEnrollments = await _context.CountAsync(_context.Enrollments.Where(e => e.UserId == userId), cancellationToken);
        
        var wallet = await _context.FirstOrDefaultAsync(_context.UserWallets, w => w.UserId == userId, cancellationToken);
        var balance = wallet?.Balance ?? 0;
        

        var response = new GetStudentDashboardSummaryResponse(
            TotalCourses: totalCourses,
            TotalSessions: totalSessions,
            TotalBookings: totalBookings,
            TotalEnrollments: totalEnrollments,
            WalletBalance: balance
        );

        return Result<GetStudentDashboardSummaryResponse>.Success(response);
    }
}
