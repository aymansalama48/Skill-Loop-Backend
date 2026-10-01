using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Pagination;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionReviews;

public sealed record SessionReviewResponse(
    Guid Id,
    Guid SessionId,
    Guid UserId,
    string UserName,
    int Stars,
    string? Comment,
    DateTime CreatedAt);

public sealed record GetSessionReviewsQuery(
    Guid SessionId,
    int PageNumber,
    int PageSize) : IQuery<PagedResult<SessionReviewResponse>>;
