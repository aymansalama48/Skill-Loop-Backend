using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Sessions.Queries.Share;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Sessions.Queries.GetSessionsPaged;

public sealed class GetSessionsPagedQueryHandler : IQueryHandler<GetSessionsPagedQuery, PagedResult<SessionResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSessionsPagedQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<SessionResponse>>> Handle(GetSessionsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Sessions.AsNoTracking();

        if (request.InstructorId.HasValue)
        {
            query = query.Where(s => s.InstructorId == request.InstructorId.Value);
        }

        query = query.OrderByDescending(s => s.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var sessions = await query
            .Skip(request.Pagination.Skip)
            .Take(request.Pagination.PageSize)
            .Select(s => new SessionResponse(
                s.Id,
                s.InstructorId,
                s.OwnerId,
                s.Title,
                s.Status,
                s.CreatedAt))
            .ToListAsync(cancellationToken);

        var paginationMetadata = new PaginationMetadata
        {
            CurrentPage = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };

        var pagedResult = new PagedResult<SessionResponse>
        {
            Items = sessions,
            Pagination = paginationMetadata
        };

        return Result<PagedResult<SessionResponse>>.Success(pagedResult);
    }
}