using MediatR;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Domain.Common.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.Application.Features.Dashboards.Queries.GetAdminDashboardSummary;

internal sealed class GetAdminDashboardSummaryQueryHandler : IRequestHandler<GetAdminDashboardSummaryQuery, Result<GetAdminDashboardSummaryResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetAdminDashboardSummaryQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<GetAdminDashboardSummaryResponse>> Handle(GetAdminDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        
        var totalCourses = await _context.CountAsync(_context.Courses, cancellationToken);
        var totalSessions = await _context.CountAsync(_context.Sessions, cancellationToken);
        var totalBookings = await _context.CountAsync(_context.Bookings, cancellationToken);
        var totalEnrollments = await _context.CountAsync(_context.Enrollments, cancellationToken);
        var totalWalletBalances = await _context.UserWallets.SumAsync(w => w.Balance, cancellationToken);
        

        var response = new GetAdminDashboardSummaryResponse(
            TotalCourses: totalCourses,
            TotalSessions: totalSessions,
            TotalBookings: totalBookings,
            TotalEnrollments: totalEnrollments,
            WalletBalance: totalWalletBalances
        );

        return Result<GetAdminDashboardSummaryResponse>.Success(response);
    }
}
