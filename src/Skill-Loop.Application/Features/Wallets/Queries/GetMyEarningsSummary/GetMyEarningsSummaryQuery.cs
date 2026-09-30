using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Wallets.Queries.GetMyEarningsSummary;

[AuthenticatedOnly]
public sealed record GetMyEarningsSummaryQuery(int? Year, int? Month) : IQuery<EarningsSummaryDto>;

public sealed record EarningsSummaryDto(
    decimal TotalEarnings,
    decimal EarningsThisMonth,
    decimal EarningsLastMonth,
    IReadOnlyCollection<MonthlyEarningsDto> MonthlyHistory,
    IReadOnlyCollection<CourseEarningsDto> TopEarningCourses,
    IReadOnlyCollection<SessionEarningsDto> TopEarningSessions
);

public sealed record MonthlyEarningsDto(int Year, int Month, decimal Amount);
public sealed record CourseEarningsDto(Guid CourseId, string CourseTitle, decimal Amount);
public sealed record SessionEarningsDto(Guid SessionId, string SessionTitle, decimal Amount);
