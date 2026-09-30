using MediatR;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Dashboards.Queries.GetStudentDashboardSummary;

[AuthenticatedOnly]
public sealed record GetStudentDashboardSummaryQuery() : IRequest<Result<GetStudentDashboardSummaryResponse>>;

public sealed record GetStudentDashboardSummaryResponse(
    int TotalCourses,
    int TotalSessions,
    int TotalBookings,
    int TotalEnrollments,
    decimal WalletBalance
);
