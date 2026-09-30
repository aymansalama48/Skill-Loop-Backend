using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Notifications.DTOs;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Notifications.Queries.GetMyNotifications;

/// <summary>
/// قايمة إشعاراتي، الأحدث فوق. دي اللي بتظهر في شاشة "الإشعارات".
/// </summary>
[AuthenticatedOnly]
public sealed class GetMyNotificationsQuery : PaginationParameters, IQuery<PagedResult<NotificationDto>>
{
    public Guid UserId { get; init; }
}

public sealed class GetMyNotificationsQueryHandler : IQueryHandler<GetMyNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMyNotificationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _context.AsNoTracking(_context.Notifications)
            .Where(n => n.UserId == request.UserId);

        var totalCount = await _context.CountAsync(baseQuery, cancellationToken);

        var page = await _context.ToListAsync(
            baseQuery
                .OrderByDescending(n => n.CreatedAt)
                .Skip(request.Skip)
                .Take(request.PageSize),
            cancellationToken);

        return Result<PagedResult<NotificationDto>>.Success(new PagedResult<NotificationDto>
        {
            Items = page.Select(n => n.ToDto()).ToList(),
            Pagination = new PaginationMetadata
            {
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        });
    }
}
