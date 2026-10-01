using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionReviews;

internal sealed class GetSessionReviewsQueryHandler(
    IApplicationDbContext _dbContext,
    IUserManagementService _userManagement) : IQueryHandler<GetSessionReviewsQuery, PagedResult<SessionReviewResponse>>
{
    public async Task<Result<PagedResult<SessionReviewResponse>>> Handle(GetSessionReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Sessions
            .Where(s => s.Id == request.SessionId)
            .SelectMany(s => s.Reviews)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var reviews = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
        var users = await _userManagement.GetUsersByIdsAsync(userIds, cancellationToken);
        var usersDict = users.ToDictionary(u => u.Id, u => u.FullName ?? "Unknown User");

        var response = reviews.Select(r => new SessionReviewResponse(
            r.Id,
            r.SessionId,
            r.UserId,
            usersDict.GetValueOrDefault(r.UserId, "Unknown User"),
            r.Stars,
            r.Comment,
            r.CreatedAt
        )).ToList();

        var pagedResult = new PagedResult<SessionReviewResponse>
        {
            Items = response,
            Pagination = new PaginationMetadata
            {
                TotalCount = totalCount,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize
            }
        };

        return Result<PagedResult<SessionReviewResponse>>.Success(pagedResult);
    }
}
