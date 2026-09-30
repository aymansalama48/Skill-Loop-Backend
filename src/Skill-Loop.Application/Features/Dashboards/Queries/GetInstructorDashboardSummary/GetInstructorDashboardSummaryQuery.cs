using MediatR;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;

namespace Skill_Loop.Application.Features.Dashboards.Queries.GetInstructorDashboardSummary;

[Permission(Permissions.Dashboards.ViewInstructor)]
public sealed record GetInstructorDashboardSummaryQuery() : IRequest<Result<GetInstructorDashboardSummaryResponse>>;

public sealed record GetInstructorDashboardSummaryResponse(
    int TotalCourses,
    int TotalSessions,
    int TotalBookings,
    int TotalEnrollments,
    decimal WalletBalance
);
