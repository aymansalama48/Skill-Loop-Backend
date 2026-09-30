using MediatR;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Dashboards.Queries.GetAdminDashboardSummary;

[Permission(Permissions.Dashboards.ViewAdmin)]
public sealed record GetAdminDashboardSummaryQuery() : IRequest<Result<GetAdminDashboardSummaryResponse>>;

public sealed record GetAdminDashboardSummaryResponse(
    int TotalCourses,
    int TotalSessions,
    int TotalBookings,
    int TotalEnrollments,
    decimal WalletBalance
);
