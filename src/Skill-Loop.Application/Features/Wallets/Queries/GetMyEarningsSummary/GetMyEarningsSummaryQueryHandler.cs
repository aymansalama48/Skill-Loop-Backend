using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.User;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Enums;
using Skill_Loop.Domain.Entities.Wallets;

namespace Skill_Loop.Application.Features.Wallets.Queries.GetMyEarningsSummary;

public sealed class GetMyEarningsSummaryQueryHandler : IQueryHandler<GetMyEarningsSummaryQuery, EarningsSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetMyEarningsSummaryQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<EarningsSummaryDto>> Handle(GetMyEarningsSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId == null || userId == Guid.Empty)
        {
            return Result<EarningsSummaryDto>.Failure(UserErrors.Unauthorized);
        }

        var wallet = await _context.UserWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            return Result<EarningsSummaryDto>.Success(new EarningsSummaryDto(0, 0, 0, [], [], []));
        }

        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var allEarnings = await _context.WalletTransactions
            .AsNoTracking()
            .Where(t => t.WalletId == wallet.Id && t.Type == TransactionType.CreditReward && t.Amount > 0)
            .ToListAsync(cancellationToken);

        var teachingEarnings = allEarnings
            .Where(t => t.Description != null && (t.Description.Contains("Course enrollment") || t.Description.Contains("Live session completion")))
            .ToList();

        var totalEarnings = teachingEarnings.Sum(t => t.Amount);
        
        var earningsThisMonth = teachingEarnings
            .Where(t => t.OccurredAt >= thisMonthStart)
            .Sum(t => t.Amount);

        var earningsLastMonth = teachingEarnings
            .Where(t => t.OccurredAt >= lastMonthStart && t.OccurredAt < thisMonthStart)
            .Sum(t => t.Amount);

        var monthlyHistory = teachingEarnings
            .GroupBy(t => new { t.OccurredAt.Year, t.OccurredAt.Month })
            .Select(g => new MonthlyEarningsDto(g.Key.Year, g.Key.Month, g.Sum(t => t.Amount)))
            .OrderByDescending(g => g.Year).ThenByDescending(g => g.Month)
            .Take(12)
            .ToList();

        var courseTransactions = teachingEarnings
            .Where(t => t.Description != null && t.Description.Contains("Course enrollment"))
            .GroupBy(t => t.ReferenceId)
            .Select(g => new { ReferenceId = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.Amount)
            .Take(5)
            .ToList();

        var courseIds = courseTransactions.Select(c => c.ReferenceId).ToList();
        var courses = await _context.Courses
            .Where(c => courseIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Title, cancellationToken);

        var topCourses = courseTransactions
            .Where(c => courses.ContainsKey(c.ReferenceId))
            .Select(c => new CourseEarningsDto(c.ReferenceId, courses[c.ReferenceId], c.Amount))
            .ToList();

        var sessionTransactions = teachingEarnings
            .Where(t => t.Description != null && t.Description.Contains("Live session completion"))
            .GroupBy(t => t.ReferenceId)
            .Select(g => new { ReferenceId = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.Amount)
            .Take(5)
            .ToList();

        var sessionIds = sessionTransactions.Select(c => c.ReferenceId).ToList();
        var sessions = await _context.Sessions
            .Where(s => sessionIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Title, cancellationToken);

        var topSessions = sessionTransactions
            .Where(s => sessions.ContainsKey(s.ReferenceId))
            .Select(s => new SessionEarningsDto(s.ReferenceId, sessions[s.ReferenceId], s.Amount))
            .ToList();

        var summary = new EarningsSummaryDto(
            totalEarnings,
            earningsThisMonth,
            earningsLastMonth,
            monthlyHistory,
            topCourses,
            topSessions
        );

        return Result<EarningsSummaryDto>.Success(summary);
    }
}
